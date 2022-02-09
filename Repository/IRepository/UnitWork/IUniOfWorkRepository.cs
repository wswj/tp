using SqlSugar;

namespace FluUrl.Repository.IRepository.UnitWork
{
    public interface IUniOfWorkRepository
    {
        SqlSugarClient GetDbClient();

        void BeginTran();

        void CommitTran();
        void RollbackTran();
    }
}
