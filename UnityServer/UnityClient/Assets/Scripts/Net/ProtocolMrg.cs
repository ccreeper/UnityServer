using SimpleServer.Business;
using SimpleServer.Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolMrg 
{
    /// <summary>
    /// 请求密钥
    /// </summary>
    public static void SecretRequest()
    {
        MsgSecret msg = new MsgSecret();
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgSecret, (resmsg) =>
        {
            NetManager.Instance.SetSecretKey(((MsgSecret)resmsg).Srcret);
            Debug.Log("获取密钥：" + ((MsgSecret)resmsg).Srcret);
        });
    }

    //测试分包
    public static void SocketTest() {
        MsgTest msg = new MsgTest();
        //分包测试
        //msg.ReqContent = "aaazzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
        //    "zzzzzzzzzzzzzweeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        msg.ReqContent = "aaaaaaaaa";
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgTest, (resmsg) =>
        {
            Debug.Log("测试回调：" + ((MsgTest)resmsg).RecContent);
        });
    }


    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="type"></param>
    /// <param name="username"></param>
    /// <param name="pwd"></param>
    /// <param name="code"></param>
    /// <param name="callback">参数与协议返回的参数相一致</param>
    public static void Register(RegisterType type, string username, string pwd, string code, Action<RegisterResult> callback) {
        MsgRegister msg = new MsgRegister();
        msg.RegisterType = type;
        msg.Account = username;
        msg.Password = pwd;
        msg.Code = code;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgRegister, (resmsg) =>
        {
            MsgRegister msgRegister = (MsgRegister)resmsg;
            callback(msgRegister.Result);
        });
    }

    /// <summary>
    /// 登陆
    /// </summary>
    /// <param name="type"></param>
    /// <param name="username"></param>
    /// <param name="pwd"></param>
    /// <param name="callback"></param>
    public static void Login(LoginType type, string username, string pwd, Action<LoginResult,string> callback) {
        MsgLogin msg = new MsgLogin();
        msg.Account = username;
        msg.Password = pwd;
        msg.LoginType = type;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgLogin, (resmsg) =>
        {
            MsgLogin msgLogin = (MsgLogin)resmsg;
            callback(msgLogin.Result, msgLogin.Token);
        });
    }
}
