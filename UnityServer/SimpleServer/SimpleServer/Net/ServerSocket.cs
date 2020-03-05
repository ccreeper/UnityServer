using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    class ServerSocket : Singleton<ServerSocket>
    {
        //公钥
        public static string PUBLIC_KEY = "OceanServer";
        //私钥
        public static string SERCET_KEY = "Ocean_Up&&NB!";

        //端口
        private const int DEFAULT_PORT = 8011;
        //服务器socket
        private static Socket m_ListenSocket;

        //心跳包间隔时间
        public static long m_PingInterval = 30;

        //当前所有socket集合，包括服务器和已连接客户端
        private static List<Socket> m_CheckReadList = new List<Socket>();

        //客户端Socket
        private static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();

        //临时的列表
        private static List<ClientSocket> m_TempList = new List<ClientSocket>();

#if DEBUG
        private string m_IpStr = "127.0.0.1";
#else
        //实际服务器地址（本地ip，而不是公网ip）
        private string m_IpStr="172.62.21.32";
#endif

        public void Init()
        {
            //创建服务器Socket
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, DEFAULT_PORT);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定监听端口
            m_ListenSocket.Bind(ipEndPoint);

            Debug.LogInfo("服务器正在监听{0}端口...", m_ListenSocket.LocalEndPoint.ToString());

            while (true)
            {


                ResetCheckRead();
                try
                {
                    //等待时间1000微秒
                    Socket.Select(m_CheckReadList, null, null, 1000);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                for (int i = 0; i < m_CheckReadList.Count; i++)
                {
                    Socket s = m_CheckReadList[i];
                    if (s == m_ListenSocket)
                    {
                        //有新客户端连接
                        ReadListen(m_ListenSocket);
                    }
                    else
                    {
                        //接收到客户端发送的消息
                        ReadClient(s);
                    }
                }

                //检测心跳包是否超时
                long timeNow = GetTimeStamp();
                m_TempList.Clear();
                foreach (ClientSocket clientSocket in m_ClientDic.Values)
                {
                    if (timeNow - clientSocket.LastPingTime > m_PingInterval * 4)
                    {
                        Debug.Log(clientSocket.Socket.RemoteEndPoint.ToString() + "Ping 超时");
                        //CloseClient(clientSocket);
                        //不能直接在循环中删除，防止遍历中集合发生变化
                        m_TempList.Add(clientSocket);
                    }
                }

                foreach (ClientSocket client in m_TempList)
                {
                    CloseClient(client);
                }
                m_TempList.Clear();
            }

        }

        //处理更新当前存在的Socket集合
        public void ResetCheckRead()
        {
            m_CheckReadList.Clear();
            m_CheckReadList.Add(m_ListenSocket);
            foreach (Socket s in m_ClientDic.Keys)
            {
                m_CheckReadList.Add(s);
            }
        }

        //响应服务器Socket可读事件，当服务器可读时，表示有客户端连接
        private void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();
                ClientSocket clientSocket = new ClientSocket();
                clientSocket.Socket = client;
                clientSocket.LastPingTime = GetTimeStamp();

                m_ClientDic.Add(client, clientSocket);

                Debug.Log("客户端{0}连接，当前在线{1}个客户端...", client.LocalEndPoint.ToString(), m_ClientDic.Count);
            }
            catch (SocketException e)
            {
                Debug.LogError("Accept fail:" + e.ToString());
            }
        }

        //接收客户端消息
        private void ReadClient(Socket socket)
        {
            ClientSocket client = m_ClientDic[socket];
            ByteArray readBuffer = client.ReadBuffer;
            // 后续处理，根据协议解析再下发到客户端
            int count = 0;
            //如果上一次接收占满了缓冲区
            if (readBuffer.Remain <= 0) {
                //数据移动
                OnReceiveData(client);
                readBuffer.CheckAndReads();

                //剩余空间依然不足，表示数据长度过大，扩容
                while (readBuffer.Remain <= 0) {
                    int expandSize = readBuffer.Length < ByteArray.DEFAULT_SIZE ? ByteArray.DEFAULT_SIZE : readBuffer.Length;
                    readBuffer.Resize(expandSize);
                }
            }
            try
            {
                count = socket.Receive(readBuffer.Bytes,readBuffer.WriteIndex,readBuffer.Remain,0);
            }
            catch (SocketException e)
            {
                Debug.LogError("Receive fail :" + e);
                CloseClient(client);
            }
            //count<=0表示客户端断开连接
            if (count <= 0) {
                CloseClient(client);
                return;
            }

            readBuffer.WriteIndex += count;
            //解析信息，分包、黏包处理
            OnReceiveData(client);

            readBuffer.CheckAndReads();
        }

        void OnReceiveData(ClientSocket client)
        {
            
        }

        private void CloseClient(ClientSocket client)
        {
            client.Socket.Close();
            m_ClientDic.Remove(client.Socket);
            Debug.Log("客户端{0}断开连接，当前在线数{1}...", client.Socket.LocalEndPoint.ToString(), m_ClientDic.Count);
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}
