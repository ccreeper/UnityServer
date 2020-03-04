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
        public static string PUBLICKEY = "OceanServer";
        //密钥，随时间变化
        public static string SECRETKEY = "Ocean_Up&&NB!";

        //端口
        private const int PORT = 8011;


#if DEBUG
        private string m_IpStr = "127.0.0.1";

#else
        //对应实际服务器地址(本地ip，而不是公共ip)
        private string m_IpStr = "172.45.124.2";
#endif 
        //服务器监听socket
        private static Socket m_ListenSocket;

        //客户端socket集合
        private static List<Socket> m_CheckReadList = new List<Socket>();

        public void Init() {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, PORT);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);
            
        }
    }
}
