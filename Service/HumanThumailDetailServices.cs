using FluUrl.Entity;
using FluUrl.Repository.IRepository;
using FluUrl.Repository.IRepository.UnitWork;
using FluUrl.Service.IService;

namespace FluUrl.Service
{
    public class HumanThumailDetailServices :BaseServices<HumanThumailDetail>,IHumanThumailDetailServices
    {
        private readonly IUniOfWorkRepository uniOfWork;
        private readonly IHumanThumailDetailRepository _dal;
        public HumanThumailDetailServices(IUniOfWorkRepository uniOfWorkRepository,IHumanThumailDetailRepository dal) {
            BaseDal = dal;
            uniOfWork = uniOfWorkRepository;
            _dal = dal;
        }
    }
}
