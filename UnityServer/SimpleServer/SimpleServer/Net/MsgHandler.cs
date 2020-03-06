using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public partial class MsgHandler
    {
        /// <summary>
        /// 所有的协议处理函数名都与协议名一一对应
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msgBase"></param>
        public static void MsgSecret(ClientSocket client, MsgBase msgBase) {
            MsgSecret msgSecret = (MsgSecret)msgBase;
            //请求密钥
            msgSecret.Srcret = ServerSocket.SERCET_KEY;
            //再分发回客户端
            ServerSocket.Send(client, msgSecret);
        }
    }
}
