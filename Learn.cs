using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Archmal
{
    public static partial class Eval
    {
        static string NewEvalFilePath = @"./eval/NEW_KKPP.bin";
        
        public static void Clear()
        {
            Array.Clear(KKPP, 0, 12 * 12 * (int)EvalIndex.FE_END * (int)EvalIndex.FE_END);
        }

        public static void FVFileCreate()
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
				// Evaluator.LearnPhase2(); // renovate weight
				Console.WriteLine("finish " + DateTime.Now);
				Console.WriteLine();
			}
			// LearnPhase1(learnDep, start, end); // J'&& Accuracy check

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