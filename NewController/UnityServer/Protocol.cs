using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;


namespace UnityServer
{
    enum ProtocolNames
    {
        SetClient = 0,
        Connect,
        Test,
        ConnectUnityServer,
        GetClientStateRequest,
        GetClientStateResponse,
        RunClientUnityRequest,
        RunClientUnityResponse,
        CloseUnityRequest,
        CloseUnityResponse,
    }
    class Protocol
    {
        TcpClient sockClient;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
        bool isConnect;

        private static readonly Lazy<Protocol> _instance = new Lazy<Protocol>(() => new Protocol());
        public static Protocol Instance { get { return _instance.Value; } }

        public void Init()
        {
            sockClient = new TcpClient();
            Thread receiveMessageThread = new Thread(ReceiveMessage);

            isConnect = false;

            receiveMessageThread.Start();

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.KeyChar == 's')
                {
                    sw.WriteLine(((int)ProtocolNames.GetClientStateRequest).ToString());
                    sw.Flush();
                }
                if (info.KeyChar == 'a')
                {
                    sw.WriteLine(((int)ProtocolNames.RunClientUnityRequest).ToString());
                    sw.Flush();
                }
                if (info.KeyChar == 'c')
                {
                    sw.WriteLine(((int)ProtocolNames.CloseUnityRequest).ToString());
                    sw.Flush();
                }
            }
        }

        private void ReceiveMessage()
        {
            string strRecvMsg;

            while (true)
            {
                if (!isConnect)
                {
                    Thread.Sleep(100);
                    try
                    {
                        sockClient = new TcpClient();
                        sockClient.Connect("192.168.0.2", 9090); //소켓생성,커넥트
                        isConnect = true;
                        ns = sockClient.GetStream();
                        sr = new StreamReader(ns);
                        sw = new StreamWriter(ns);

                        sw.WriteLine(((int)ProtocolNames.ConnectUnityServer).ToString());
                        sw.Flush();
                    }
                    catch
                    {
                        isConnect = false;
                    }
                    continue;
                }
                try
                {
                    strRecvMsg = sr.ReadLine();
                    if (strRecvMsg.Split(':')[0] == ((int)ProtocolNames.GetClientStateResponse).ToString())
                    {
                        // Cmd 켜져있는 클라이언트 인덱스 배열.
                        // -1은 무시, 이외는 서버에서 체크하는 인덱스. (인덱스는 서버에서 정함)
                        // ex) [0, -1] -> 0번 PC가 대기중 / [0, 2] -> 0번, 2번 PC가 대기중
                        int[] readyClientArr = new int[2] { 
                            int.Parse(strRecvMsg.Split(':')[1].Split(',')[0]),
                            int.Parse(strRecvMsg.Split(':')[1].Split(',')[1])
                        };
                        Console.WriteLine(strRecvMsg.Split(':')[1].Split(',')[0] + ", " + strRecvMsg.Split(':')[1].Split(',')[1]);
                    }
                    else if (strRecvMsg.Split(':')[0] == ((int)ProtocolNames.GetClientStateResponse).ToString())
                    {
                        Console.WriteLine(strRecvMsg.Split(':')[1]);
                    }
                }
                catch
                {
                    Console.WriteLine("Server Disconnect");
                    isConnect = false;
                    sockClient.Close();
                }
            }
        }
    }
}
