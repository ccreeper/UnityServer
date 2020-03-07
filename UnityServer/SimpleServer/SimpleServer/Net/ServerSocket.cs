using ServerBase;
using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
        private const int DEFAULT_PORT = 8888;
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
            //监听数量
            m_ListenSocket.Listen(50000);

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

                Debug.Log("客户端{0}连接，当前在线{1}个客户端...", client.RemoteEndPoint.ToString(), m_ClientDic.Count);
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


        /// <summary>
        /// 接收数据处理
        /// </summary>
        /// <param name="client"></param>
        void OnReceiveData(ClientSocket client)
        {
            ByteArray readBuffer = client.ReadBuffer;

            //判断数据长度
            if (readBuffer.Length <= 4 || readBuffer.ReadIndex < 0)
                return;
            int readIndex = readBuffer.ReadIndex;
            byte[] bytes = readBuffer.Bytes;
            //readIndex开始的4个字节就是协议头，代表了整个协议的长度
            int bodyLength = BitConverter.ToInt32(bytes, readIndex);
            if (bodyLength + 4 > readBuffer.Length)
            {
                //协议头+协议长度 > 实际内容长度  ，表示信息不全，分包的情况，等待下次接收后拼接
                //协议头+协议长度 < 实际内容长度  , 可能存在黏包
                return;
            }
            //越过协议头移动到包体的位置
            readBuffer.ReadIndex += 4;

            //解析协议名
            int nameCount = 0;
            ProtocolEnum proto = ProtocolEnum.None;
            try
            {
                proto = MsgBase.DecodeName(readBuffer.Bytes, readBuffer.ReadIndex, out nameCount);
            }
            catch (Exception e)
            {
                Debug.LogError("解析协议名出错" + e);
                CloseClient(client);
                return;
            }

            if (proto == ProtocolEnum.None) {
                Debug.LogError("OnReceiveData 解析协议名失败");
                CloseClient(client);
                return;
            }

            readBuffer.ReadIndex += nameCount;
            //解析协议体
            int bodyCount = bodyLength - nameCount;
            MsgBase msg = null;
            try
            {
                msg = MsgBase.Decode(proto, readBuffer.Bytes, readBuffer.ReadIndex, bodyCount);
                if (msg == null) {
                    Debug.LogError("协议解析出错...");
                    CloseClient(client);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("{0}协议解析出错" + e, proto.ToString());
                CloseClient(client);
                return;
            }
            readBuffer.ReadIndex += bodyCount;

            readBuffer.CheckAndReads();

            //分发消息，通过反射调用对应的协议处理方法，所以最好保持方法与协议名一致
            MethodInfo mi = typeof(MsgHandler).GetMethod(proto.ToString());
            object[] obj = { client, msg };
            if (mi != null)
            {
                mi.Invoke(null, obj);
            }
            else {
                Debug.LogError("{0}协议未找到相应的方法..."+proto.ToString());
            }


            //继续读取,length>4表示还未读取完，黏包的情况
            if (readBuffer.Length > 4) {
                OnReceiveData(client);
            }

        }

        public static void Send(ClientSocket socket ,MsgBase msg) {
            if (socket == null || !socket.Socket.Connected)
                return;
            try
            {
                //生成发送的完整协议：协议头+协议名称+协议内容
                byte[] nameBytes = MsgBase.EncodeName(msg);
                byte[] bodyBytes = MsgBase.Encode(msg);
                int len = nameBytes.Length + bodyBytes.Length;
                byte[] headBytes = BitConverter.GetBytes(len);
                byte[] sendBytes = new byte[headBytes.Length + len];
                Array.Copy(headBytes, 0, sendBytes, 0, headBytes.Length);
                Array.Copy(nameBytes, 0, sendBytes, headBytes.Length, nameBytes.Length);
                Array.Copy(bodyBytes, 0, sendBytes, headBytes.Length + nameBytes.Length, bodyBytes.Length);
                try
                {
                    socket.Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
                }
                catch (SocketException e)
                {
                    Debug.LogError("Socket BeginSend Error:" + e);
                }

            }
            catch (SocketException e)
            {
                Debug.LogError("Socket发送数据出错..." + e);
            }
        }

        private void CloseClient(ClientSocket client)
        {
            m_ClientDic.Remove(client.Socket);
            Debug.Log("客户端{0}断开连接，当前在线数{1}...", client.Socket.RemoteEndPoint.ToString(), m_ClientDic.Count);
            client.Socket.Close();
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}
