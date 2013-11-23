using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace ClipboardShare
{
    public delegate void ReceiveMsgHandler(Message msg);
    class NetworkService
    {
        private static byte[] result = new byte[1024];
        private static int controlProt = 8885;
        private static int fileProt = 8884;
        private static Object syncRoot = new Object();
        private static NetworkService instance;
        private static bool _connected = false;
        static Socket serverSocket;
        static Socket clientSocket;

        static Socket fileSocket;

        private static ReceiveMsgHandler handler;

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

        public int SendFile(string file)
        {
            //IPEndPoint clientpoint = clientSocket.RemoteEndPoint as IPEndPoint;
            //clientpoint.Port = 8884;
            //using(TcpClient tmpclient = new TcpClient(clientpoint))
            //{
            //    using(NetworkStream networkStream = tmpclient.GetStream())
            //    {
            //        byte[] dataToSend = File.ReadAllBytes(file);
            //        networkStream.Write(dataToSend, 0, dataToSend.Length);
            //        networkStream.Flush();
            //    }
            //}
            Thread myThread = new Thread(ListenFileConnect);
            myThread.IsBackground = true;
            myThread.Start(file);
            return 0;
        }

        private int initial()
        {
            IPAddress ip = IPAddress.Parse("218.193.187.165");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, controlProt));  //绑定IP地址：端口  
            serverSocket.Listen(10);    //设定最多10个排队连接请求
            Console.WriteLine("Create Control Server successfully", serverSocket.LocalEndPoint.ToString());

            fileSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            fileSocket.Bind(new IPEndPoint(ip, fileProt));
            fileSocket.Listen(2);
            Console.WriteLine("Create File Server successfully", fileSocket.LocalEndPoint.ToString());

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
                    //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
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

        private static void ListenFileConnect(object filepath)
        {
            string file = (string)filepath;
            Socket fileClientSocket;
            fileClientSocket = fileSocket.Accept();

            Console.WriteLine("Receive a TCP connection");

            int ret = fileClientSocket.Send(File.ReadAllBytes(file));
            Console.WriteLine("Send " + ret + " bytes");

            fileClientSocket.Shutdown(SocketShutdown.Both);
            fileClientSocket.Close();
        }

        public void setDelegate(ReceiveMsgHandler myhandler){
            handler = myhandler;
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
                    Message msg = new Message(Encoding.UTF8.GetString(result, 0, receiveNumber));
                    handler(msg);
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
            if (clientSocket != null)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            if(serverSocket != null)
                serverSocket.Close();
        }
    }
}
