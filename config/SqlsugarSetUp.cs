using FluUrl.Helper;
using FluUrl.Utils;
using SqlSugar;

namespace FluUrl.config
{
    public static class SqlsugarSetUp
    {
        public static void AddSqlSugarSetup(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            string connectionString = AppSettingsHelper.GetContent("ConnectionStrings", "SqlConnection");
            string dbTypeString = AppSettingsHelper.GetContent("ConnectionStrings", "DbType");
            //获取数据类型
            var dbType = dbTypeString == DbType.MySql.ToString() ? DbType.MySql : DbType.SqlServer;
            var connectionConfig = new ConnectionConfig() {
                ConnectionString = connectionString,
                DbType = dbType,
                IsAutoCloseConnection = false,
            InitKeyType =InitKeyType.Attribute,
            /*设置redis作为sqlsugar二级缓存.暂时不使用
             * ConfigureExternalServices =new ConfigureExternalServices() {
                DataInfoCacheService=myCache
            }*/
            };
            services.AddScoped<ISqlSugarClient>(o => {
                var db = new SqlSugarClient(connectionConfig);

                //日志处理
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    //获取sql
                    Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                };
                //db.Aop.OnLogExecuted = (sql, pars) => //SQL执行完事件
                //{

                //};
                //db.Aop.OnLogExecuting = (sql, pars) => //SQL执行前事件
                //{

                //};
                db.Aop.OnError = (exp) =>//执行SQL 错误事件
                {
                    NLogUtil.WriteFileLog(NLog.LogLevel.Error, LogType.Other, "SqlSugar", "执行SQL错误事件", exp);
                };

                return db;
            });
        }
    }
}
