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
    class ServerSocket:Singleton<ServerSocket>
    {
        //公钥
        private static string PUBLIC_KEY = "OceanServer";
        //私钥
        private static string SERCET_KEY = "Ocean_Up&&NB!";
        //端口
        private const int DEFAULT_PORT = 8011;
        //服务器socket
        private static Socket m_ListenSocket;
        //客户端socket集合
        private static List<Socket> m_CheckReadList = new List<Socket>();

#if DEBUG
        private string m_IpStr = "127.0.0.1";
#else
        //实际服务器地址（本地ip，而不是公网ip）
        private string m_IpStr="172.62.21.32";
#endif

        public void Init() {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, DEFAULT_PORT);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);

            Debug.LogInfo("服务器正在监听{0}端口...",m_ListenSocket.LocalEndPoint.ToString());

        }
    }
}
