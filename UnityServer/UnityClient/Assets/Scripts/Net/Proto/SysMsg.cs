using ProtoBuf;


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
