using FluUrl.Entity;
using FluUrl.ViewModels;

namespace FluUrl.Service.IService
{
    public interface IHumanInfoServices : IBaseServices<HumanInfo>
    {
        /// <summary>
        /// 获取指定用户的指定长度的目标列表
        /// </summary>
        /// <param name="humanId"></param>
        /// <returns></returns>
        Task<IEnumerable<Thumail>> GetHumanInfo(int humanId,int count=0);
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<IEnumerable<ShowGrilViewModel>> GetHuman(int page=1);
    }
}
