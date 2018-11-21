using System;
using System.Linq;
using System.Collections.Generic;

namespace Archmal
{
    public static class Eval
    {
        static int[] PieceValue = new int[]
        {
            0, 
            +100, +200, +220, +15000, +400,
            0, 0, 0,
            -100, -200, -220, -15000, -400,
        };

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
    }
}