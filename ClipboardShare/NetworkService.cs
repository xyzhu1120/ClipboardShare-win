using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ClipboardShare
{
    class NetworkService
    {
        private static byte[] result = new byte[1024];
        private static int myProt = 8885;
        private static Object syncRoot = new Object();
        private static NetworkService instance;
        private static bool _connected = false;
        static Socket serverSocket;
        static Socket clientSocket;

        public static bool connected
        {
            get
            {
                return _connected;
            }
        }

        public static NetworkService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NetworkService();
                    }
                }
                return instance;
            }
        }

        private NetworkService()
        {
            this.initial();
        }

        public int SendMessage(String msg)
        {
            if (clientSocket != null)
                clientSocket.Send(Encoding.UTF8.GetBytes(msg));
            else
                return -1;
            return 0;
        }

        private int initial()
        {
            IPAddress ip = IPAddress.Parse("218.193.187.165");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
            serverSocket.Listen(10);    //设定最多10个排队连接请求
            Console.WriteLine("Create Server successfully", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            Thread myThread = new Thread(ListenClientConnect);
            myThread.IsBackground = true;
            myThread.Start();
            return 0;
        }

        private static void ListenClientConnect()
        {
            Socket tmp;
            while (true)
            {

                tmp = serverSocket.Accept();
                if (clientSocket == null)
                {
                    Console.WriteLine("Receive a TCP connection");
                    clientSocket = tmp;
                    clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                    Thread receiveThread = new Thread(ReceiveMessage);
                    receiveThread.IsBackground = true;
                    receiveThread.Start(clientSocket);
                    _connected = true;
                }
                else
                {
                    tmp.Shutdown(SocketShutdown.Both);
                    tmp.Close();
                }
            }
        }

        private static void ReceiveMessage(object tmpclientSocket)
        {
            Socket myClientSocket = (Socket)tmpclientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    if (receiveNumber == 0)
                    {
                        Console.WriteLine("The connection is shutdown!");
                        myClientSocket.Close();
                        clientSocket = null;
                        break;
                    }
                    Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(result, 0, receiveNumber));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                    break;
                }
            }
        }

        ~NetworkService()
        {
            if(clientSocket != null)
                clientSocket.Close();
            if(serverSocket != null)
                serverSocket.Close();
        }
    }
}
