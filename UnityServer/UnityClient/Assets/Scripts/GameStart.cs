using SimpleServer.Business;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Instance.Connect("127.0.0.1", 8888);
        StartCoroutine(NetManager.Instance.CheckNet());
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Instance.Update();
        if (Input.GetKeyDown(KeyCode.A)){
            //黏包测试
            ProtocolMrg.SocketTest();
            //ProtocolMrg.SocketTest();
            //ProtocolMrg.SocketTest();
            //ProtocolMrg.SocketTest();
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            ProtocolMrg.Register(RegisterType.Phone, "176151539220", "password","yanzhengma",(res) =>
               {
                   if (res == RegisterResult.AlreadyExist)
                   {
                       Debug.LogError("手机号已被注册...");
                   }
                   else if (res == RegisterResult.WrongCode)
                   {
                       Debug.LogError("验证码错误...");
                   }
                   else if (res == RegisterResult.Forbidden)
                   {
                       Debug.LogError("禁止注册...");
                   }
                   else {
                       Debug.Log("注册成功...");
                   }
               });
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ProtocolMrg.Login(LoginType.Phone, "176151539220", "password", (res,token) =>
            {
                if (res == LoginResult.UserNotExist)
                {
                    Debug.LogError("用户不存在...");
                }
                else if (res == LoginResult.Failed)
                {
                    Debug.LogError("登陆失败...");
                }
                else if (res == LoginResult.WrongPwd)
                {
                    Debug.LogError("密码错误...");
                }
                else
                {
                    Debug.Log("登陆成功...");
                }
            });
        }
    }

    private void OnApplicationQuit()
    {
        NetManager.Instance.Close();
    }

}
