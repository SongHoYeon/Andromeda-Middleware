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
            string[] lines = System.IO.File.ReadAllLines(@"2020_FTX_Data\StreamingAssets\Config.yml");
            string ip = lines[2].Split(':')[1].Replace(" ", "");

            Console.SetWindowSize(40, 20);
            Protocol.Instance.Init(ip, lines[0].Split(':')[1].Replace(" ", "")[4]);
        }
    }
}
