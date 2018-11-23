using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Archmal
{
  public class Client
  {
    static Position pos;
    static TcpClient client;
    static Color myTurn;
    static bool inConnect;

    public Client(Position p)
    {
        pos = p;
    }

    public void Start(string server, int port)
    {
        client = new TcpClient();
        client.Connect(server, port);
        Console.WriteLine("client connected!!");
        SendData("LOGIN:KumasanBot");
        NetworkStream stream = client.GetStream();
        Thread thread = new Thread(o => ReceiveData((TcpClient)o));

        thread.Start(client);

        thread.Join();
        client.Client.Shutdown(SocketShutdown.Send);
        stream.Close();
        client.Close();
        Console.WriteLine("disconnect from server!!");
    }

    static void ReceiveData(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        Thread thread = new Thread(new ThreadStart(SendHeartBeat));
        inConnect = true;
        thread.Start();

        while (true)
        {
            try {
                byte[] receivedBytes = new byte[1024];
                int byte_count = stream.Read(receivedBytes, 0, receivedBytes.Length);
                if (byte_count == 0)
                    break;
                string receiveiveStr = Encoding.UTF8.GetString(receivedBytes, 0, byte_count);
                HandleReceiveData(receiveiveStr);
            }
            catch (System.IO.IOException)
            {
                break;
            }
        }

        inConnect = false;
        thread.Join();
    }

    static void SendData(string msg)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer, 0, buffer.Length);
    }

    static void SendHeartBeat()
    {
        while (inConnect)
        {
            SendData("\n");
            Thread.Sleep(30000);
        }
    }

    static void HandleReceiveData(string str)
    {
        string[] messages = str.Split('\n');

        foreach (string cmd in messages)
        {
            if (cmd.StartsWith("Your_Turn"))
            {
                if (cmd.Substring(10, 1) == "+") 
                {
                    myTurn = Color.BLACK;
                }
                else {
                    myTurn = Color.WHITE;
                }
            }
            else if (cmd.StartsWith("END"))
            {
                SendData("AGREE");
            }
            else if (cmd.StartsWith("START"))
            {
                if (myTurn == Color.BLACK)
                    SendData(think());
            }
            else if (cmd.StartsWith("+") || cmd.StartsWith("-"))
            {
                pos.DoMove(new Move(cmd.Substring(1,6)));
                if (pos.SideToMove() == myTurn)
                    SendData(think());
            }
            else {
                Console.WriteLine(cmd);
            }
        }
    }

    static string think()
    {
        Console.WriteLine("think start");
        string str = pos.SideToMove() == Color.BLACK ? "+" : "-";
        str += Search.iteration(pos);
        Console.WriteLine("think end");
        return str;
    }
  }
}