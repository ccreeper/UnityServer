using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySql
{
    public class MySqlMgr : Singleton<MySqlMgr>
    {

#if DEBUG
        private const string connectingStr = "server=localhost;uid=root;pwd= ;database=ocean";
#else
        //对应服务器配置
        private const string connectingStr = "server=localhost;uid=root;pwd= ;database=ocean";
#endif
        public SqlSugarClient SqlSugarDB = null;

        public void Init() {
            SqlSugarDB = new SqlSugarClient(
                new ConnectionConfig() {
                    ConnectionString = connectingStr,
                    DbType = DbType.MySql,      //数据库类型
                    IsAutoCloseConnection = true,   //自动释放数据在事务执行结束后
                    InitKeyType = InitKeyType.Attribute
                });
#if DEBUG
            //打印信息
            SqlSugarDB.Aop.OnLogExecuting = (sql,pars) =>
            {
                Console.WriteLine(sql + "\r\n" +
                    SqlSugarDB.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
#endif    
        }
    }
}
