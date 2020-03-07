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
        NetManager.Instance.SendMsg(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgSecret, (resmsg) =>
        {
            NetManager.Instance.SetSecretKey(((MsgSecret)resmsg).Srcret);
            Debug.Log("获取密钥：" + ((MsgSecret)resmsg).Srcret);
        });


    }
}
