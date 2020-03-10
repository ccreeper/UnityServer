using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Business
{
    //注册类型
    public enum RegisterType
    {
        Phone,
        Mail,
    }

    //登陆类型
    public enum LoginType
    {
        Phone,
        Mail,
        Wechat,
        QQ,
        Token,
    }

    public enum RegisterResult
    {
        Success,
        Failed,
        AlreadyExist,   //已存在
        WrongCode,  //验证码错误
        Forbidden,
    }

    public enum LoginResult {
        Success,
        Failed,
        WrongPwd,
        UserNotExist,
        TimeoutToken,
    }
}
