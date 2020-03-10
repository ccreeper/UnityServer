using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public class ClientSocket
    {
        //保存客户端的Socket
        public Socket Socket { get; set; }

        //最后响应连接的时间
        public long LastPingTime { get; set; }

        public ByteArray ReadBuffer = new ByteArray();

        public int UserId = 0;

    }
}
