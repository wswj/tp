using FluUrl.Repository.IRepository;
using FluUrl.Service.IService;
using FluUrl.ViewModels.UI;
using Microsoft.AspNetCore.Mvc;

namespace FluUrl.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HumanInfoController : ControllerBase
    {
        private readonly IHumanInfoServices _humanInfoServices;
        private readonly IRedisOperationRepository _redisOperationRepository;
        public HumanInfoController(IHumanInfoServices humanInfoServices,IRedisOperationRepository redisOperationRepository)
        {
            _humanInfoServices = humanInfoServices;
            _redisOperationRepository = redisOperationRepository;
        }
        [HttpGet]
        public async Task<WebApiCallBack> Index([FromQuery] int page=1)
        {
            var jm = new WebApiCallBack();
            var humanList= await _humanInfoServices.GetHuman(page);
            jm.status = true;
            jm.data = humanList;
            jm.otherData = await _redisOperationRepository.Get<int>("page");
            return jm;
        }
    }
}

