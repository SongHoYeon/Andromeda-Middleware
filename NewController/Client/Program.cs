using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Client
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
