using FluUrl.Repository.IRepository.UnitWork;
using FluUrl.Utils;
using SqlSugar;
using NLog;
namespace FluUrl.Repository
{
    public class UniOfWorkRepository : IUniOfWorkRepository
    {
        private readonly ISqlSugarClient _sqlSugarClient;

        public UniOfWorkRepository(ISqlSugarClient sqlSugarClient)
        {
            _sqlSugarClient = sqlSugarClient;
        }

        /// <summary>
        ///     获取DB，保证唯一性
        /// </summary>
        /// <returns></returns>
        public SqlSugarClient GetDbClient()
        {
            // 必须要as，后边会用到切换数据库操作
            return _sqlSugarClient as SqlSugarClient;
        }

        public void BeginTran()
        {
            GetDbClient().BeginTran();
        }

        public void CommitTran()
        {
            try
            {
                GetDbClient().CommitTran(); //
            }
            catch (Exception ex)
            {
                GetDbClient().RollbackTran();
                NLogUtil.WriteFileLog(NLog.LogLevel.Error, LogType.Web, "事务提交异常", "事务提交异常", new Exception("事务提交异常", ex));
                throw;
            }
        }

        public void RollbackTran()
        {
            GetDbClient().RollbackTran();
        }
    }
}
