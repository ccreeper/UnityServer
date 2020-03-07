using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using SimpleServer.Proto;

/// <summary>
/// 协议类名与协议名一致（建议）
/// </summary>
[ProtoContract]
public class MsgSecret : MsgBase
{
    public MsgSecret()
    {
        ProtoType = ProtocolEnum.MsgSecret;
    }

    //序列化
    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)]
    public string Srcret;
}

[ProtoContract]
public class MsgPingHeart : MsgBase
{
    public MsgPingHeart()
    {
        ProtoType = ProtocolEnum.MsgPingHeart;
    }

    //序列化
    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }
    
}

[ProtoContract]
public class MsgTest : MsgBase
{
    public MsgTest()
    {
        ProtoType = ProtocolEnum.MsgTest;
    }

    //序列化
    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }


    [ProtoMember(2)]
    public string ReqContent { get; set; }


    [ProtoMember(3)]
    public string RecContent { get; set; }

}