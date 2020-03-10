using ProtoBuf;
using SimpleServer.Business;
using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[ProtoContract]
public class MsgRegister : MsgBase
{
    public MsgRegister()
    {
        ProtoType = ProtocolEnum.MsgRegister;
    }
    
    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }
    [ProtoMember(2)]
    public string Account;
    [ProtoMember(3)]
    public string Password;
    [ProtoMember(4)]
    public string Code;
    [ProtoMember(5)]
    public RegisterType RegisterType;
    //服务器返回的数据
    [ProtoMember(6)]
    public RegisterResult Result;
    
}

[ProtoContract]
public class MsgLogin : MsgBase
{
    public MsgLogin()
    {
        ProtoType = ProtocolEnum.MsgLogin;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }
    [ProtoMember(2)]
    public string Account;
    [ProtoMember(3)]
    public string Password;
    [ProtoMember(4)]
    public LoginType LoginType;
    //服务器返回的数据
    [ProtoMember(5)]
    public LoginResult Result;
    [ProtoMember(6)]
    public string Token;

}