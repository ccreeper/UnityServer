using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Proto
{
    public enum ProtocolEnum
    {
        None = 0,
        //获取密钥
        MsgSecret = 1,
        //心跳包协议
        MsgPingHeart = 2,
        //注册
        MsgRegister = 3,
        //登陆
        MsgLogin = 4,

        MsgTest =9999,
    }
}
