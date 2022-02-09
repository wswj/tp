using FluUrl.Entity;
using FluUrl.Repository.IRepository;
using FluUrl.Repository.IRepository.UnitWork;

namespace FluUrl.Repository
{
    public class HumanThumailDetailRepository:BaseRepository<HumanThumailDetail>,IHumanThumailDetailRepository
    {
        public HumanThumailDetailRepository(IUniOfWorkRepository uniOfWorkRepository) : base(uniOfWorkRepository) { }
    }
}
