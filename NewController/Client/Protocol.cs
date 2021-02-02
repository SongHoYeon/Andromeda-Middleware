using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;

namespace Client
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
        int idx;
        bool isConnect;

        private static readonly Lazy<Protocol> _instance = new Lazy<Protocol>(() => new Protocol());
        public static Protocol Instance { get { return _instance.Value; } }

        public void Init()
        {
            sockClient = new TcpClient();
            Thread receiveMessageThread = new Thread(ReceiveMessage);

            isConnect = false;

            receiveMessageThread.Start();
            
            //sr.Close();
            //sw.Close();
            //ns.Close();
            //sockClient.Close();

            //Console.WriteLine("접속 종료!");
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
                        sw.WriteLine(((int)ProtocolNames.SetClient).ToString());
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
                    if (strRecvMsg.Split(':')[0] == ((int)ProtocolNames.Connect).ToString())
                    {
                        Console.WriteLine("PC Index : " + strRecvMsg.Split(':')[1]);
                        idx = int.Parse(strRecvMsg.Split(':')[1]);
                    }
                    else if (strRecvMsg.Split(':')[0] == ((int)ProtocolNames.Test).ToString())
                    {
                        Console.WriteLine(strRecvMsg.Split(':')[1]);
                    }
                    else if (strRecvMsg.Split(':')[0] == ((int)ProtocolNames.RunClientUnityResponse).ToString())
                    {
                        Console.WriteLine("Open Unity");
                        System.Diagnostics.Process.Start("notepad.exe");
                    }
                    else if (strRecvMsg.Split(':')[0] == ((int)ProtocolNames.CloseUnityResponse).ToString())
                    {
                        Console.WriteLine("Close Unity");
                        //string processName = taskname.Replace("notepad.exe", "");

                        foreach (Process process in Process.GetProcessesByName("notepad"))
                        {
                            process.Kill();
                        }
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
