using System;

namespace Archmal
{
    class Program
    {
        static void Main(string[] args)
        {
            Int32 port;
            Int32.TryParse(args[1], out port);

            while(true)
            {
                Position pos = new Position();
                Client client = new Client(pos);
                client.Start(args[0], port);
            }
        }
    }
}
