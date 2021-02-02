using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;

namespace Server
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
    public class MyClient
    {
        public TcpClient client;
        public int idx;
        public NetworkStream ns;
        public StreamReader sr;
        public StreamWriter sw;
        public string id;

        public void Init(TcpClient client, NetworkStream ns, StreamReader sr, StreamWriter sw, string id = "", int idx = -1)
        {
            this.client = client;
            this.idx = idx;
            this.ns = ns;
            this.sr = sr;
            this.sw = sw;
            this.id = id;
        }
    }
    public class Protocol
    {
        TcpListener sockServer;
        List<MyClient> clientList;
        MyClient unityServer;
        List<Thread> receiveThreadList;
        int connecCount;

        private static readonly Lazy<Protocol> _instance = new Lazy<Protocol>(() => new Protocol());
        public static Protocol Instance { get { return _instance.Value; } }

        public void Init(string ip)
        {
            receiveThreadList = new List<Thread>();

            clientList = new List<MyClient>();
            Console.WriteLine("Server Start = " + ip + ":9090");
            sockServer = new TcpListener(IPAddress.Parse(ip), 9090); //IP, Port
            sockServer.Start();
            connecCount = 0;

            receiveThreadList.Add(new Thread(Receive));
            receiveThreadList[connecCount].Start();
        }

        private void Receive()
        {
            int threadIdx = connecCount;
            try
            {
                string strMsg;

                TcpClient client = sockServer.AcceptTcpClient();//Accept

                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);

                receiveThreadList.Add(new Thread(Receive));
                receiveThreadList[++connecCount].Start();

                while (true)
                {
                    try
                    {
                        strMsg = sr.ReadLine();
                        if (strMsg == null)
                            continue;
                        if (strMsg.Split(':')[0] == ((int)ProtocolNames.SetClient).ToString())
                        {
                            sw.WriteLine(((int)ProtocolNames.Connect).ToString() + ':' + (connecCount - 1));
                            sw.Flush();

                            Console.WriteLine("UserId : " + strMsg.Split(':')[1] + " Idx : " + (connecCount - 1) + " PC Connect");
                            MyClient currentClient = new MyClient(); ;
                            currentClient.Init(client, ns, sr, sw, strMsg.Split(':')[1], connecCount - 1);
                            clientList.Add(currentClient);
                        }
                        else if (strMsg.Split(':')[0] == ((int)ProtocolNames.ConnectUnityServer).ToString())
                        {
                            unityServer = new MyClient(); ;
                            unityServer.Init(client, ns, sr, sw);
                            Console.WriteLine("Unity Server Connect");
                        }
                        else if (strMsg.Split(':')[0] == ((int)ProtocolNames.GetClientStateRequest).ToString())
                        {
                            string indexData = "";
                            int count = 0;
                            foreach (MyClient item in clientList)
                            {
                                if (!item.client.Connected)
                                    continue;
                                count++;
                                indexData += item.id + ">" + item.idx;
                                if (count == 1)
                                    indexData += ',';
                            }
                            if (count == 1)
                            {
                                if (indexData[0] == '1')
                                    indexData += "0>" + -1;
                                else
                                    indexData += "1>" + -1;
                            }
                            else if (count == 0)
                            {
                                indexData = "0>-1,1>-1";
                            }
                            unityServer.sw.WriteLine(((int)ProtocolNames.GetClientStateResponse).ToString() + ':' + indexData);
                            unityServer.sw.Flush();
                        }
                        else if (strMsg.Split(':')[0] == ((int)ProtocolNames.RunClientUnityRequest).ToString())
                        {
                            foreach (MyClient item in clientList)
                            {
                                if (!item.client.Connected)
                                    continue;
                                if (item.idx == int.Parse(strMsg.Split(':')[1].Split(',')[0]) || item.idx == int.Parse(strMsg.Split(':')[1].Split(',')[1]))
                                {
                                    item.sw.WriteLine(((int)ProtocolNames.RunClientUnityResponse).ToString());
                                    item.sw.Flush();
                                }
                            }
                        }
                        else if (strMsg.Split(':')[0] == ((int)ProtocolNames.CloseUnityRequest).ToString())
                        {
                            foreach (MyClient item in clientList)
                            {
                                if (!item.client.Connected)
                                    continue;
                                if (item.idx == int.Parse(strMsg.Split(':')[1].Split(',')[0]) || item.idx == int.Parse(strMsg.Split(':')[1].Split(',')[1]))
                                {
                                    item.sw.WriteLine(((int)ProtocolNames.CloseUnityResponse).ToString());
                                    item.sw.Flush();
                                }
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Disconnect");
                        sw.Close();
                        sr.Close();
                        ns.Close();
                        client.Close();
                        receiveThreadList[threadIdx].Abort();
                    }
                }

                sockServer.Stop();

                Console.WriteLine("Client 연결 종료!");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
