using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Archmal
{
    public static class EvalIndex
    {
        public const int ListSize = 6;

        public const int F_STAND_PAWN = 0;
        public const int E_STAND_PAWN   = F_STAND_PAWN   +  2;
        public const int F_STAND_BISHOP = E_STAND_PAWN   +  2;
        public const int E_STAND_BISHOP = F_STAND_BISHOP +  2;
        public const int F_STAND_ROOK   = E_STAND_BISHOP +  2;
        public const int E_STAND_ROOK   = F_STAND_ROOK   +  2;
        public const int FE_STAND_END   = E_STAND_ROOK   +  2;
        public const int F_PAWN         = FE_STAND_END   + 12; 
        public const int E_PAWN         = F_PAWN         + 12;
        public const int F_BISHOP       = E_PAWN         + 12;
        public const int E_BISHOP       = F_BISHOP       + 12;
        public const int F_ROOK         = E_BISHOP       + 12;
        public const int E_ROOK         = F_ROOK         + 12;
        public const int F_GOLD         = E_ROOK         + 12;
        public const int E_GOLD         = F_GOLD         + 12;
        public const int FE_END         = E_GOLD         + 12;

        public static int[] IndexArray = new int[]
        {
            0, F_PAWN, F_BISHOP, F_ROOK, 0, F_GOLD,
            0, 0,
            0, E_PAWN, E_BISHOP, E_ROOK, 0, E_GOLD
        };

        public static int[] IndexStandArray = new int[]
        {
            0, F_STAND_PAWN, F_STAND_BISHOP, F_STAND_ROOK, 0, 0,
            0, 0,
            0, E_STAND_PAWN, E_STAND_BISHOP, E_STAND_ROOK, 0, 0
        };

        public static int[] SquareIndex = new int[]
        {
            -1, -1, -1, -1,
            -1, -1, -1, -1,
             0,  1,  2, -1,
             3,  4,  5, -1,
             6,  7,  8, -1,
             9, 10, 11, -1,
            -1, -1, -1, -1,
            -1, -1, -1, -1
        };

        public static int SquareToIndex(Square s)
        {
            return SquareIndex[(int)s];
        }

        public static int SquareToIndex(int s)
        {
            return SquareIndex[s];
        }
    }

    public static class Eval
    {
        static int[,,,] KKPP = new int [12, 12, (int)EvalIndex.FE_END, (int)EvalIndex.FE_END];

        static int[] PieceValue = new int[]
        {
            0, 
            +100, +200, +220, +15000, +400,
            0, 0, 0,
            -100, -200, -220, -15000, -400,
        };

        public static List<int> MakeList(Position pos, ref int material)
        {
            var list = new List<int>(6);
            int nlist = 0;

            for (Square sq = Square.SQ_09; sq <= Square.SQ_23; ++sq)
            {
                int piece = pos.SquareIs(sq);

                if ( piece == Piece.Empty
                  || piece == Piece.Wall)
                    continue;

                material += PieceValue[piece];

                if (Piece.Abs(piece) == Piece.BK)
                    continue;

                list[nlist++] = EvalIndex.IndexArray[piece] + EvalIndex.SquareToIndex(sq);
            }

            for (int p = Piece.BP; p < Piece.BK; ++p)
            {
                for (int i = 1; i <= pos.Stand(Color.BLACK, p); ++i)
                {
                    material += PieceValue[p];
                    list[nlist++] = EvalIndex.IndexStandArray[p] + i;
                }
                for (int i = 1; i <= pos.Stand(Color.WHITE, p); ++i)
                {
                    material -= PieceValue[p];
                    list[nlist++] = EvalIndex.IndexStandArray[p + 8] + i;
                }
            }

            Debug.Assert(nlist == 6);

            return list;
        }

        public static int evaluate(Position pos)
        {
            int value = 0;

            for (Square sq = Square.SQ_09; sq <= Square.SQ_23; ++sq)
            {
                int piece = pos.SquareIs(sq);

                if ( piece == Piece.Empty
                  || piece == Piece.Wall)
                    continue;

                value += PieceValue[piece];
            }

            for (int p = Piece.BP; p < Piece.BK; ++p)
                if (pos.Stand(Color.BLACK, p) != 0)
                    value += PieceValue[p] * pos.Stand(Color.BLACK, p);

            for (int p = Piece.BP; p < Piece.BK; ++p)
                if (pos.Stand(Color.WHITE, p) != 0)
                    value -= PieceValue[p] * pos.Stand(Color.WHITE, p);

            return pos.SideToMove() == Color.BLACK ? +value : -value;
        }

        public static int evaluate_kkpp(Position pos)
        {
            int value = 0;
            int material = 0;
            int value_kkpp = 0;

            int bk = EvalIndex.SquareToIndex(pos.KingPos((int)Color.BLACK));
            int wk = EvalIndex.SquareToIndex(pos.KingPos((int)Color.WHITE));

            var list = MakeList(pos, ref material);

            for (int i = 0; i < EvalIndex.ListSize; ++i)
            {
                int k = list[i];
                for (int j = 0; j < i; ++j)
                {
                    int l = list[j];
                    value_kkpp += KKPP[bk, wk, k, l];
                }
            }

            value = value_kkpp + value;

            return pos.SideToMove() == Color.BLACK ? +value : -value;
        }
    }
}