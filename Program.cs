using System;

namespace Archmal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ServerGC = " + System.Runtime.GCSettings.IsServerGC);
            Console.WriteLine("Usage: command [command options]");
			Console.WriteLine("command:");
            Console.WriteLine("\tclient    -- client-mode");
            Console.WriteLine("\tlearn  -- [learnDepth][iterate] Bonanza Method");
            Console.WriteLine();

            Eval.Init();

            var sub = Console.ReadLine().Split(' ');
            if (sub[0] == "lean")
            {
                new Learn().LearnAll(int.Parse(sub[1]), int.Parse(sub[2]));
                Position pos = new Position();
                Search.Iteration(pos);
            }
            else if (sub[0] == "client")
            {
                while(true)
                {
                    Position pos = new Position();
                    Client client = new Client(pos);
                    client.Start(args[0], Int32.Parse(sub[1]));
                }
            }
        }
    }
}
