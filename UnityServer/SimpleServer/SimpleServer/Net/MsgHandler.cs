using ServerBase;
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

        // 所有的协议处理函数名都与协议名一一对应


        /// <summary>
        /// 密钥请求处理
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


        /// <summary>
        /// 心跳包处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msgBase"></param>
        public static void MsgPingHeart(ClientSocket client, MsgBase msgBase)
        {
            client.LastPingTime = ServerSocket.GetTimeStamp();
            MsgPingHeart msPong = new MsgPingHeart();
            ServerSocket.Send(client, msPong);
        }

        /// <summary>
        /// 分包测试
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msgBase"></param>
        public static void MsgTest(ClientSocket client, MsgBase msgBase)
        {
            MsgTest msgTest = (MsgTest)msgBase;
            Debug.Log(msgTest.ReqContent);
            //分包测试
            //msgTest.RecContent = "服务器返回数据："+
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
            //"zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";

            msgTest.RecContent = "服务器返回数据：";
            ServerSocket.Send(client, msgTest);
        }
    }
    
}
