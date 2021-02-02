using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(40, 20);
            Protocol.Instance.Init();
        }
    }
}
