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

        static Move bestRootMove;

        private static int search(Position pos, int alpha, int beta, int depth, int ply, bool pvNode)
        {
            bool rootNode = pvNode && ply == 0;
            int bestValue = -Value.Infinite;
            int value = 0;
            int moveCount;
            int newPly = ply + 1;
            Move bestMove;

            if (depth <= 0)
            {
                return Eval.evaluate(pos);
            }

            GenerateMoves gm = new GenerateMoves(pos);
            List<Move> moves = gm.GenerateAllMoves();

            moveCount = 0;

            foreach (Move m in moves)
            {
                int newDepth = depth - 1;
                bool win = pos.DoMove(m);

                moveCount++;

                if (win)
                {
                    pos.UndoMove(m);
                    if (rootNode)
                        bestRootMove = m;
                    return Value.EvalInfinite;
                }
                
                value = -search(pos, -beta, -alpha, newDepth, newPly, true);

                pos.UndoMove(m);

                if (rootNode)
                {
                     if (moveCount == 1 || value > alpha)
                    {
                        bestRootMove = m;
                    }
                }

                if (value > bestValue)
                {
                    bestValue = value;

                    if (value > alpha)
                    {
                        bestMove = m;

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

            return bestValue;
        }

        public static string iteration(Position pos)
        {
            int value = search(pos, -Value.Infinite, +Value.Infinite, 5, 0, true);
            Console.WriteLine("best move is " + bestRootMove.ToSfen());
            Console.WriteLine("value is " + value);
            return bestRootMove.ToSfen();
        }
    }
}