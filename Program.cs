#define LEARN

using System;

namespace Archmal
{
    class Program
    {
        static void Main(string[] args)
        {
#if LEARN
            new Learn().LearnPhase1(3);
#else
            Int32 port;
            Int32.TryParse(args[1], out port);

            Eval.Init();

            while(true)
            {
                Position pos = new Position();
                Client client = new Client(pos);
                client.Start(args[0], port);
            }
#endif
        }
    }
}
