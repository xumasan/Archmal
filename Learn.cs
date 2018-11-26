using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archmal
{
    public static partial class Eval
    {
        static string NewEvalFilePath = @"./eval/NEW_KKPP.bin";
        static double[,,,] pData;
        public static void LearnPhase2()
		{
			pData = new double[12, 12, (int)EvalIndex.FE_END, (int)EvalIndex.FE_END];
			for (int i = 0; i < 32; i++) // 32-iterate
			{
				Console.Write("renovate " + i + "steps\r");
				Array.Clear(pData, 0, pData.Length);
				Parallel.ForEach(PVFileParse1(), game => PVFileParse2(game));
				RenovateParam();
			}
			FVFileCreate();
			pData = null; // free
		}

		static IEnumerable<List<string>> PVFileParse1()
		{
			using (var reader = new StreamReader(@"./learn/TempPV"))
			{
				var game = new List<string>();
				string s;
				while ((s = reader.ReadLine()) != null)
				{
					if (s == "0") // end of game
					{
						yield return game;
						game = new List<string>();
					}
					else game.Add(s);
				}
			}
		}

		static void PVFileParse2(List<string> game)
		{
			var pos = new Position();
			List<Move> postive = null;
			List<List<Move>> negative = null;

			foreach (var sub in game)
			{
				var list = new List<int>();
				foreach (var s in sub.Split(' ')) 
                    list.Add(int.Parse(s));

				if (list[0] == +1)
				{
					if (postive != null)
					{
						CalcDeriv(pos, postive, negative);
						pos.DoMove(postive[0]); // HACK
					}
					postive = new List<Move>();
					negative = new List<List<Move>>();
					for (int i = 1; i < list.Count; i++) 
                        postive.Add(new Move(list[i]));
				}
				else // list[0] == -1 => negative
				{
					var negaPV = new List<Move>();
					for (int i = 1; i < list.Count; i++) 
                        negaPV.Add(new Move(list[i]));
					negative.Add(negaPV);
				}
			}
		}
		static void CalcDeriv(Position pos, List<Move> positive, List<List<Move>> negative)
		{
            Color pTurn = pos.SideToMove();
            // 局面を進めて評価値を得る
			foreach (var m in positive) pos.DoMove(m);
			int bestVal = pTurn == pos.SideToMove() ? Evaluate_kkpp(pos) : -Evaluate_kkpp(pos);
			foreach (var m in positive.Reverse<Move>()) pos.UndoMove(m);

			double sumdT = 0.0;
			foreach (var pv in negative)
			{
				foreach (var m in pv) pos.DoMove(m);
				int val = pTurn == pos.SideToMove() ? Evaluate_kkpp(pos) : -Evaluate_kkpp(pos);
				double dT = Learn.dSigmoid(val - bestVal);
				IncParam(pos, -dT, pTurn);
				foreach (var m in pv.Reverse<Move>()) pos.UndoMove(m);
				sumdT += dT;
			}
			foreach (var m in positive) pos.DoMove(m);
			IncParam(pos, +sumdT, pTurn);
			foreach (var m in positive.Reverse<Move>()) pos.UndoMove(m);
		}

		static void IncParam(Position pos, double dinc, Color turn)
		{
			if (turn == Color.WHITE) dinc = -1 * dinc;

			int dummy = 0;
			int bk = EvalIndex.SquareToIndex(pos.KingPos(Color.BLACK));
            int wk = EvalIndex.SquareToIndex(pos.KingPos(Color.WHITE));
			var list = MakeList(pos, ref dummy);

            for (int i = 0; i < EvalIndex.ListSize; ++i)
            {
                int k = list[i];
                for (int j = 0; j < i; ++j)
                {
                    int l = list[j];
                    pData[bk, wk, k, l] += dinc;
                }
            }
		}

        // 正規化
		static void RenovateParam()
		{
			var random = new Random();
			const double L1 = 0.010; // lasso  = was 1.00
			for (int i = 0; i < 12; ++i) 
                    for (int j = 0; j < 12; ++j)
                        for (int k = 0; k < (int)EvalIndex.FE_END; ++k) 
                            for (int l = 0; l < (int)EvalIndex.FE_END; ++l) 
                            {
                                int w = KKPP[i, j, k, l];
                                int sign = Math.Sign(pData[i, j, k, l] - Math.Sign(w) * L1);
                                KKPP[i, j, k, l] += sign * random.Next(0, FVScale / 8);

                                if (w * KKPP[i, j, k, l] < 0) KKPP[i, j, k, l] = 0; // clipping
                            }
		}

		static void FVFileCreate()
		{
			using (var write = new BinaryWriter(File.OpenWrite(NewEvalFilePath)))
			{
				for (int i = 0; i < 12; ++i) 
                    for (int j = 0; j < 12; ++j)
                        for (int k = 0; k < (int)EvalIndex.FE_END; ++k) 
                            for (int l = 0; l < (int)EvalIndex.FE_END; ++l) 
                                write.Write(KKPP[i, j, k, l]);
			}
		}
    }

    public class Learn
    {
        public void LearnAll(int learnDep, int iterate)
        {
            var time = Stopwatch.StartNew();
            Console.WriteLine("learnDepth = " + learnDep);
			Console.WriteLine("iterate    = " + iterate);
			Console.WriteLine("start time " + DateTime.Now);

            for (int step = 0; step < iterate; ++step) // learn
			{
				Console.WriteLine(step + " step(s)");
				LearnPhase1(learnDep); // pv create
				Eval.LearnPhase2(); // renovate weight
				Console.WriteLine("finish " + DateTime.Now);
				Console.WriteLine();
			}
			LearnPhase1(learnDep); // J'&& Accuracy check

			Console.WriteLine("end time   " + DateTime.Now);
			Console.WriteLine("total time " + time.Elapsed);
        }

        public void LearnPhase1(int learnDep)
		{
            var PVFile = new System.IO.StreamWriter(@"./learn/TempPV");
			int total = 0;
			double error = 0;
			double accuracyCount = 0;
			int count = 0;

            // 学習用データ
            var teacher = new GameDB();
            
            // 学習開始
            foreach (var g in teacher.games)
            {
                var pos = new Position();
                var pvStr = new List<string>();

                foreach (var positive in g.moves)
                {
                    // 違法手がある
                    if (!pos.IsLegalMove(positive))
                    {
                        Console.WriteLine(pos.PrintPosition());
                        Console.WriteLine("illegalMove is : " + positive.ToSfen());
                    }
                    Debug.Assert(pos.IsLegalMove(positive));

                    total++;
					List<Move> bestPV;
                    pos.DoMove(positive);
                    // 教師データでの正解手
                    int bestValue = -Search.FullSearch(pos, -Value.Infinite, +Value.Infinite, learnDep, 0, true, bestPV = new List<Move>());
                    pos.UndoMove(positive);

                    // 詰み周りは学習しない
                    if (Math.Abs(bestValue) == Value.EvalInfinite)
                        break;
                    
                    // 正解手
                    string p = "+1 " + positive.HashCode();
					bestPV.ForEach(m => p += " " + m.HashCode());
					pvStr.Add(p);

                    // Others
                    bool positiveIsBest = true;
					foreach (Move move in new GenerateMoves(pos).GenerateAllMoves().Where(m => m != positive))
					{
						const int FVWindow = 200;
						int alpha = bestValue - FVWindow;
						int beta = bestValue + FVWindow;

						List<Move> PV;
						pos.DoMove(move);
						int value = -Search.FullSearch(pos, -beta, -alpha, learnDep, 0, true, PV = new List<Move>());
						pos.UndoMove(move);

                        // 教師の手が最善になっていない
						if (value >= bestValue) positiveIsBest = false;
                        // windowsに収まっている
						if (alpha < value && value < beta)
						{
							string n = "-1 " + move.HashCode();
							PV.ForEach(m => n += " " + m.HashCode());
							pvStr.Add(n);
						}

						if (value >= beta) error += 1.0;
						else if (alpha < value && value < beta) error += Sigmoid(value - bestValue);
						else if (value <= alpha) error += 0.0;
					}
                    if (positiveIsBest) accuracyCount++;
                    // 局面を進める
					pos.DoMove(positive);
                }

                // 対局が終了　
                pvStr.Add("0");
				pvStr.ForEach(s => PVFile.WriteLine(s));
				Console.Write((count++ * 1.0 / teacher.games.Count).ToString("P") + "\r");
            }
            Console.WriteLine("Error    = " + (error / total).ToString("f")); // calc J'
			Console.WriteLine("Accuracy = " + (accuracyCount / total).ToString("p")); // calc accuracy
			PVFile.Close();
        }

        public static double Sigmoid(double x)
		{
			return 1 / (1 + Math.Exp(-3 * x / 128)); // FVWindow = 200
		}

		public static double dSigmoid(double x)
		{
			return Sigmoid(x) * (1 - Sigmoid(x)); // max => 0.25
		}
    }
}