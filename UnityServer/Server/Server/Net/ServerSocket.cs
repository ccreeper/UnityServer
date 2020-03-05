using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Net
{
    class ServerSocket : Singleton<ServerSocket>
    {
        //公钥
        public static string PUBLIC_KEY = "OceanServer";
        //密钥
        public static string SECRET_KEY = "Ocean_Up&&NB!";
        //绑定的端口
        private const int DEFAULT_PORT = 8011;


#if DEBUG
        private string m_IpStr = "127.0.0.1";
#else
        //实际的服务器地址（本地ip，不是公网ip）
        private string m_IpStr = "172.123.52.12";
#endif
        //服务器socket
        private static Socket m_ListenSocket;

        //客户端socket连接的集合
        private static List<Socket> m_CheckReadList = new List<Socket>();

        public void Init() {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, DEFAULT_PORT);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);

        }
    }
}
