using System;
using System.Linq;
using System.Collections.Generic;

namespace Archmal
{
    public class Value {
        public const int Infinite = Int32.MaxValue;
        public const int EvalInfinite = Int32.MaxValue - 128;
    }

    public class Search {

        public static int FullSearch(Position pos, int alpha, int beta, int depth, int ply, bool pvNode, List<Move> PV)
        {
            bool rootNode = pvNode && ply == 0;
            int bestValue = -Value.Infinite;
            int value = 0;
            int moveCount;
            int newPly = ply + 1;
            var bestPV = new List<Move>();

            if (depth <= 0)
            {
                return QSearch(pos, alpha, beta, 0, ply, pvNode, PV);
            }

            GenerateMoves gm = new GenerateMoves(pos);
            List<Move> moves = gm.GenerateAllMoves();

            moveCount = 0;

            foreach (Move m in moves)
            {
                List<Move> newPV = null;

                int newDepth = depth - 1;
                bool win = pos.DoMove(m);

                moveCount++;

                if (win)
                {
                    pos.UndoMove(m);
                    PV.Add(m);
                    return Value.EvalInfinite;
                }
                
                value = -FullSearch(pos, -beta, -alpha, newDepth, newPly, true, newPV = new List<Move>{m});

                pos.UndoMove(m);

                if (value > bestValue)
                {
                    bestValue = value;
                    bestPV = newPV;

                    if (value > alpha)
                    {
                        if (pvNode && value < beta) // alpha の更新
                            alpha = value;
                        else
                        {
                            // beta cut
                            break;
                        }
                    }
                }
            }

            PV.AddRange(bestPV);
            return bestValue;
        }

        public static int QSearch(Position pos, int alpha, int beta, int depth, int ply, bool pvNode, List<Move> PV)
        {
            bool rootNode = pvNode && ply == 0;
            int value = 0;
            int moveCount;
            int newPly = ply + 1;
            var bestPV = new List<Move>();

            int standPat = Eval.Evaluate_kkpp(pos);
			int bestValue = standPat;
			
            if (bestValue >= beta) 
                return bestValue; // standPat
			
            if (depth <= -6) 
                return bestValue; // limit

            GenerateMoves gm = new GenerateMoves(pos);
            List<Move> moves = gm.GenerateCaptureMoves();

            moveCount = 0;

            foreach (Move m in moves)
            {
                List<Move> newPV = null;

                int newDepth = depth - 1;
                bool win = pos.DoMove(m);

                moveCount++;

                if (win)
                {
                    pos.UndoMove(m);
                    PV.Add(m);
                    return Value.EvalInfinite;
                }
                
                value = -FullSearch(pos, -beta, -alpha, newDepth, newPly, true, newPV = new List<Move>{m});

                pos.UndoMove(m);

                if (value > bestValue)
                {
                    bestValue = value;
                    bestPV = newPV;

                    if (value > alpha)
                    {
                        if (pvNode && value < beta) // alpha の更新
                            alpha = value;
                        else
                        {
                            // beta cut
                            break;
                        }
                    }
                }
            }

            PV.AddRange(bestPV);
            return bestValue;
        }

        public static string Iteration(Position pos)
        {
            var PV = new List<Move>();
            for (int depth = 1; depth < 6; ++depth)
            {
                PV.Clear();
                int value = FullSearch(pos, -Value.Infinite, +Value.Infinite, depth, 0, true, PV);
                Console.WriteLine("info depth " + depth);
                Console.WriteLine("best move is " + PV[0].ToSfen());
                Console.Write("pv");
				PV.ForEach(x => Console.Write(" " + x.ToSfen()));
                Console.WriteLine("\nvalue is " + value);
            }
            return PV[0].ToSfen();
        }
    }
}