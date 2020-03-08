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
}
