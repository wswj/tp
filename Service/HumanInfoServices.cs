using Flurl.Http;
using FluUrl.Entity;
using FluUrl.Helper;
using FluUrl.Repository;
using FluUrl.Repository.IRepository;
using FluUrl.Repository.IRepository.UnitWork;
using FluUrl.Service.IService;
using FluUrl.ViewModels;
using HtmlAgilityPack;
using JasonSoft.Net.JsHttpClient.Http;
using Newtonsoft.Json;

namespace FluUrl.Service
{
    public class HumanInfoServices : BaseServices<HumanInfo>, IHumanInfoServices
    {
        //依赖注入
        private readonly IHumanInfoRepository _dal;
        private readonly IUniOfWorkRepository _unitOfWork;
        private readonly IJsHttpClient _client;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRedisOperationRepository _redisOperationRepository;
        private readonly IMinIOServices _minIOServices;
        public HumanInfoServices(IUniOfWorkRepository unitOfWork, IMinIOServices minIOServices, IHumanInfoRepository dal, IJsHttpClient jsHttpClient, IWebHostEnvironment webHostEnvironment, IRedisOperationRepository redisOperationRepository)
        {
            _dal = dal;
            BaseDal = dal;
            _unitOfWork = unitOfWork;
            _client = jsHttpClient;
            _webHostEnvironment = webHostEnvironment;
            _redisOperationRepository = redisOperationRepository;
            _minIOServices = minIOServices;
        }

        public async Task<IEnumerable<ShowGrilViewModel>> GetHuman(int page = 1)
        {
            HtmlDocument html = new HtmlDocument();
            string urlString = page == 1 ? "https://www.xsnvshen.com/girl/" : $"https://www.xsnvshen.com/girl?p={page}";
            var request = new JsHttpRequest { Uri = urlString, Timeout = 10000 };
            var response = await _client.SendAsync(request);
            html.LoadHtml(response.Html);
            HtmlNode htmlNodes = html.DocumentNode;
            //获取名称
            var humanNameNodes = htmlNodes.SelectNodes("//body/div/div/ul/li/a");
            var PageNode = htmlNodes.SelectNodes("//body/div/div/div/a");
            var totalPage = 0;
            foreach (var node in PageNode)
            {
                if (node.Attributes["class"] != null && node.Attributes["class"].Value == "a1")
                {
                    Console.WriteLine(node.InnerText);
                    totalPage = node.InnerText.Replace("共", "").Replace("位", "").ObjectToInt();
                    break;
                }
            }
            await _redisOperationRepository.Set("page", totalPage, TimeSpan.FromSeconds(60.0));
            var humanList = new List<ShowGrilViewModel>();
            if (humanNameNodes.Any())
            {
                foreach (var humanNameNode in humanNameNodes)
                {
                    var grilInfo = new ShowGrilViewModel();
                    //获取详情页显示的图片
                    var detailUrl = humanNameNode.Attributes["href"].Value;
                    var id = detailUrl.Split('/').LastOrDefault();
                    var cacheResult = await _redisOperationRepository.Get("listinfo:" + id);
                    //如果缓存中没有缓存
                    if (string.IsNullOrEmpty(cacheResult))
                    {
                        try
                        {
                            var name = humanNameNode.Attributes["title"].Value;


                            var mainImg = "https:" + humanNameNode.ChildNodes[1].Attributes["src"].Value;
                            var test = await mainImg.WithHeaders(new
                            {
                                Accept = @"image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8",
                                Host = @"img.xsnvshen.com",
                                Referer = @"https://www.xsnvshen.com/gril/",
                            }).GetStreamAsync();
                            //DownloadFileAsync(_webHostEnvironment.ContentRootPath + "wwwroot/MainImg/", detailUrl.Replace("/", "") + ".jpg");
                            //var result = File.Open(_webHostEnvironment.ContentRootPath + "wwwroot/MainImg/"+detailUrl.Replace("/", "") + ".jpg",FileMode.Open);
                            var result = test;
                            //读取下载之后的文件转换为formfile再添加到formfilecollection中
                            var ms = new MemoryStream();
                            try
                            {
                                result.CopyTo(ms);
                                var formFile = new FormFile(ms, 0, ms.Length, "test", "test")
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "image/jpeg"
                                };
                                var forms = new FormFileCollection();
                                forms.Add(formFile);
                                //back中存储的是minio的图片路径
                                var back = await _minIOServices.UploadAsync(forms);
                                string url = "";
                                if (back.status)
                                {
                                    url = (back.data as FileModel).Url;
                                }
                                grilInfo.Name = name;
                                grilInfo.DetailUrl = detailUrl;
                                grilInfo.MainImg = url;
                                //存储完毕后添加到缓存中
                                await _redisOperationRepository.Set("listinfo:" + id, grilInfo, TimeSpan.MaxValue);
                            }
                            catch (Exception e)
                            {
                                ms.Dispose();
                                throw;
                            }
                            finally
                            {
                                ms.Dispose();
                            }

                            //mainImg = "http://localhost:5555/MainImg/" + detailUrl.Replace("/", "") + ".jpg";
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                    else
                    {
                        grilInfo = JsonConvert.DeserializeObject<ShowGrilViewModel>(cacheResult);
                    }


                    humanList.Add(grilInfo);
                }
            }
            return humanList;
        }

        public async Task<IEnumerable<Thumail>> GetHumanInfo(int humanId, int count = 0)
        {
            HtmlDocument html = new HtmlDocument();
            string urlString = $"https://www.xsnvshen.com/girl/{humanId}";
            var request = new JsHttpRequest { Uri = urlString, Timeout = 1000 };
            var response = await _client.SendAsync(request);
            html.LoadHtml(response.Html);
            HtmlNode htmlNodes = html.DocumentNode;
            //获取名称
            var humanNameNodes = htmlNodes.SelectNodes("//body/div/div/div/div/div/h2");
            var humanName = "";
            if (humanNameNodes.Any())
            {
                humanName = humanNameNodes.FirstOrDefault().InnerText;
            }
            else
            {
                humanName = "未知";
            }
            //获取介绍
            var humanDesc = "";
            var descs = htmlNodes.SelectNodes("//body/div/div/div/div/div/p");
            if (descs.Any())
            {
                humanDesc = descs.FirstOrDefault().InnerText;
            }
            else
            {
                humanDesc = "详情未知";
            }
            //获取信息 //body/div/div/div/div/div/ul/li 两个span bas-title bas-cont 34567 12
            // //body/div/div/div/div/div/ul/li/a
            var humanInfoNodes = htmlNodes.SelectNodes("//body/div/div/div/div/div/ul/li");
            var birthday = humanInfoNodes[3].LastChild.InnerText;
            var xz = humanInfoNodes[4].LastChild.InnerText;
            var sx = humanInfoNodes[5].LastChild.InnerText;
            var sg = humanInfoNodes[6].LastChild.InnerText;
            var weight = humanInfoNodes[7].LastChild.InnerText;
            var total = humanInfoNodes[12].LastChild.InnerText;

            //获取当前用户的图片列表
            var imgList = htmlNodes.SelectNodes("//body/div/div/div/div/div/ul/li/a");
            List<Thumail> thumails;
            return null;
        }
    }
}
