using MySql;
using MySql.MySQLData;
using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Business
{
    public class UserManager:Singleton<UserManager>
    {
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="type">注册类型</param>
        /// <param name="uname">用户名</param>
        /// <param name="pwd">密码</param>
        /// <param name="code">验证码</param>
        /// <returns></returns>
        public RegisterResult Register(RegisterType type,string uname,string pwd,out string token/*,string code*/) {
            token = "";
            try
            {
                int count = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it => it.Username == uname).Count();
                if (count > 0)
                    return RegisterResult.AlreadyExist;
                User user = new User();
                switch (type)
                {
                    //验证码通过协议申请，申请完后在邮箱中保存，在规定时间内验证
                    case RegisterType.Phone:
                        //TODO 验证手机验证码，错误返回RegisterResult.WrongCode
                        user.Logintype = LoginType.Phone.ToString();
                        break;
                    case RegisterType.Mail:
                        //TODO 邮箱验证码验证，错误返回RegisterResult.WrongCode
                        user.Logintype = LoginType.Mail.ToString();
                        break;
                }
                user.Username = uname;
                user.Password = pwd;
                user.Token = Guid.NewGuid().ToString();
                user.Logindate = DateTime.Now;
                token = user.Token;
                MySqlMgr.Instance.SqlSugarDB.Insertable(user).ExecuteCommand();
                return RegisterResult.Success;
            }
            catch (Exception e)
            {
                Debug.LogError("注册失败：" + e.ToString());
                return RegisterResult.Failed;
            }
        }

        public LoginResult Login(LoginType type, string uname, string pwd,out string token, out int uid) {
            uid = 0;
            token = "";
            try
            {
                User user = null;
                switch (type)
                {
                    case LoginType.Phone:
                    case LoginType.Mail:
                        user = MySqlMgr.Instance.SqlSugarDB.Queryable<User>()
                            .Where(it => it.Username == uname).Single();
                        break;
                    case LoginType.Wechat:
                    case LoginType.QQ:
                        //TODO 使用QQ等第三方登陆，需要多存一个unionid,再判断是否 == username
                        break;
                    case LoginType.Token:
                        user = MySqlMgr.Instance.SqlSugarDB.Queryable<User>()
                            .Where(it => it.Username == uname).Single();
                        break;
                }

                if (user == null)
                {
                    // QQ等第三方首次登陆时，相当于注册
                    if (type == LoginType.QQ || type == LoginType.Wechat)
                    {
                        user = new User();
                        user.Username = uname;
                        user.Password = pwd;
                        user.Logintype = type.ToString();
                        user.Token = Guid.NewGuid().ToString();
                        user.Logindate = DateTime.Now;
                        // 存储Unionid = username
                        token = user.Token;
                        //插入并返回id
                        uid = MySqlMgr.Instance.SqlSugarDB.Insertable(user).ExecuteReturnIdentity();
                        return LoginResult.Success;
                    }
                    else {
                        return LoginResult.UserNotExist;
                    }
                }
                else {
                    if (type != LoginType.Token)
                    {
                        if (user.Password != pwd)
                        {
                            return LoginResult.WrongPwd;
                        }
                        if (type == LoginType.Phone) {

                        } else if (type == LoginType.Mail) {

                        }
                    }
                    else {
                        if (user.Token != pwd) {
                            return LoginResult.TimeoutToken;
                        }
                    }
                    user.Token = Guid.NewGuid().ToString();
                    user.Logindate = DateTime.Now;
                    token = user.Token;
                    MySqlMgr.Instance.SqlSugarDB.Updateable(user).ExecuteCommand();
                    uid = user.Id;
                    return LoginResult.Success;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("登陆失败：" + e.ToString());
                return LoginResult.Failed;
            }
        }

    }
}
