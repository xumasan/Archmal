using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Archmal
{
    public class GenerateMoves {

        private Position pos;

        public GenerateMoves(Position p) {
            pos = p;
        }

        public List<Move> GenerateAllMoves() {

            List<Square> empties = new List<Square>();
            List<Move> moves = new List<Move>();
            for (Square from = Square.SQ_09; from <= Square.SQ_23; ++from)
            {
                int piece = pos.SquareIs(from);
                if (piece == Piece.Empty)
                {
                    empties.Add(from);
                    continue;
                }
                if ( piece == Piece.Wall
                  || Piece.ColorIs(piece) != pos.SideToMove())
                  continue;
                
                foreach (int inc in Piece.Inc[piece])
                {
                  Square to = from + inc;
                  int cap = pos.SquareIs(to);
                  bool pro = false;

                  if (  cap == Piece.Wall 
                    || (cap != Piece.Empty && Piece.ColorIs(cap) == pos.SideToMove()))
                    continue;

                  if (Piece.CanPromote(piece, to))
                    pro = true;
                  
                  moves.Add(new Move(from, to, pro, false));
                }

                for (int pc = Piece.BP; pc <= Piece.BR; ++pc)
                {
                    if (pos.Stand(pos.SideToMove(), pc) == 0)
                        continue;
                    
                    foreach (Square to in empties)
                    {
                        moves.Add(new Move((Square)pc, to, false, true));
                    }
                }
            }

            return moves;
        }

        public List<Move> GenerateCaptureMoves() {

            List<Square> empties = new List<Square>();
            List<Move> moves = new List<Move>();
            for (Square from = Square.SQ_09; from <= Square.SQ_23; ++from)
            {
                int piece = pos.SquareIs(from);
                if (piece == Piece.Empty)
                {
                    empties.Add(from);
                    continue;
                }
                if ( piece == Piece.Wall
                  || Piece.ColorIs(piece) != pos.SideToMove())
                  continue;
                
                foreach (int inc in Piece.Inc[piece])
                {
                  Square to = from + inc;
                  int cap = pos.SquareIs(to);
                  bool pro = false;

                  if (  cap == Piece.Wall 
                    ||  cap == Piece.Empty
                    || (cap != Piece.Empty && Piece.ColorIs(cap) == pos.SideToMove()))
                    continue;

                  if (Piece.CanPromote(piece, to))
                    pro = true;
                  
                  moves.Add(new Move(from, to, pro, false));
                }
            }

            return moves;
        }
    }
}