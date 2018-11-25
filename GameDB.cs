using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Archmal
{
    public class Game
    {
        public Game()
        {
            moves = new List<Move>();
        }
        public List<Move> moves;
    }
    public class GameDB
    {
        const string FilePath = @"./db/kif.txt";
        public GameDB()
        {
            games = new List<Game>();
            using (var reader = new StreamReader(FilePath))
			{
                string s;
				while ((s = reader.ReadLine()) != null)
				{
                    var g = new Game();
                    string[] moves;
                    moves = s.Replace("\"", "").Split("	");
                    
                    for (int i = 0; i < moves.Count() - 1; ++i)
                    {
                        g.moves.Add(new Move(WarsConverter(moves[i].Split(",")[0])));
                    }
                    games.Add(g);
				}
			}
        }

        string WarsConverter(string warsFormat)
        {
            string RankChar  = " abcd";
            string fFile = warsFormat.Substring(1,1);
            string fRank = warsFormat.Substring(2,1);
            string tFile = warsFormat.Substring(3,1);
            string tRank = warsFormat.Substring(4,1);
            string kind  = warsFormat.Substring(5,2);

            string move = String.Empty;
            // drop
            if (fFile == "0")
            {
                switch (kind)
                {
                    case "FU":
                    move = "h*";
                    break;
                    case "HI":
                    move = "k*";
                    break;
                    case "KA":
                    move = "z*";
                    break;
                    default:
                    Debug.Assert(false);
                    break;
                }
            }
            else {
                move = fFile + RankChar[int.Parse(fRank)];
            }

            move += tFile + RankChar[int.Parse(tRank)];

            if (kind == "TO")
                move += "+";
            else 
                move += " ";

            Console.WriteLine(warsFormat);
            Console.WriteLine(move);
            Console.ReadKey();

            return move;
        }

        List<Game> games;
    }
}