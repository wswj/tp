using FluUrl.Entity;
using FluUrl.Repository.IRepository;
using FluUrl.Repository.IRepository.UnitWork;

namespace FluUrl.Repository
{
    public class HumanInfoRepository : BaseRepository<HumanInfo>, IHumanInfoRepository
    {
        //base(unitOfWork)调用父类有一个参数的构造函数
        public HumanInfoRepository(IUniOfWorkRepository unitOfWork) : base(unitOfWork)
        {
        }
    }
}
