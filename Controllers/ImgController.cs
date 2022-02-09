using Flurl.Http;
using FluUrl.Helper;
using FluUrl.Repository.IRepository;
using FluUrl.Service.IService;
using FluUrl.Utils;
using FluUrl.ViewModels.UI;
using HtmlAgilityPack;
using JasonSoft.Net.JsHttpClient.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace FluUrl.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImgController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IJsHttpClient _client;
        private readonly ILogger<ImgController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRedisOperationRepository _redisOperationRepository;
        private readonly IHumanInfoServices _humanInfoServices;
        private HtmlDocument htmlDocument;
        private readonly IMinIOServices _minIOServices;
        private readonly IHttpContextAccessor _http;
        public ImgController(ILogger<ImgController> logger, IHttpContextAccessor http, IHttpClientFactory httpClientFactory, IJsHttpClient jsHttpClient, IWebHostEnvironment webHostEnvironment, IRedisOperationRepository redisOperationRepository,IHumanInfoServices human,IMinIOServices minIOServices)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _client = jsHttpClient;
            _webHostEnvironment = webHostEnvironment;
            _redisOperationRepository = redisOperationRepository;
            _humanInfoServices = human;
            _minIOServices = minIOServices;
            _http = http;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async void Get()
        {
            htmlDocument = new HtmlDocument();
            const string urlString = "https://www.xsnvshen.com/girl/22162";
            var request = new JsHttpRequest { Uri = urlString,Timeout=1000 };
            var response = await _client.SendAsync(request);
            htmlDocument.LoadHtml(response.Html);
            
            HtmlNode htmlNode = htmlDocument.DocumentNode;
            var list=htmlNode.SelectNodes("//div/div/div/div/div/ul/li/a");
            //��ȡ�������ҳ��ĺϼ�url
            List<string> urlList=new List<string>();
            foreach (HtmlNode node in list) {
                string url = node.Attributes["href"].Value;
                if (url.Contains("album")) {
                    urlList.Add("https://www.xsnvshen.com"+url);
                }
                
            }
            var uList = urlList.Take(200).ToList();
            var childmap = new Dictionary<string,List<string>>();
            //�����Ӻϼ�
            foreach (var url in uList) {
                Random random = new Random();
                var s = random.Next(5,10);

                //Thread.Sleep(s*100);
                var childList = new List<string>();
                //��ȡ�Ӽ�����ϸ��Ϣ
                var request1 = new JsHttpRequest { Uri = url };
                var response1 = await _client.SendAsync(request1);
                htmlDocument.LoadHtml(response1.Html);
                HtmlNode htmlNode1 = htmlDocument.DocumentNode;
                var list1 = htmlNode1.SelectNodes("//ul/li/div/img");
                //��ȡ��ǰ�����ͼƬ�б�
                try
                {
                    foreach (HtmlNode node in list1)
                    {
                        var childUrl = node.Attributes["data-original"].Value;
                        childList.Add("https:" + childUrl);
                    }
                }
                catch (Exception)
                {
                    NLogUtil.WriteAll(NLog.LogLevel.Error, LogType.Web, "ʧ��", $"��ȡ���ʧ��https://www.xsnvshen.com/album/{url}");
                    continue;
                }
                childmap.Add(url.Split("/").Last(), childList);
                //childList.Clear();
            }
            var count = 0;
            var totalCount = 0;
            foreach (var url in childmap) {
                totalCount=totalCount+url.Value.Count();
                
                }
            Console.WriteLine("����Ϊ"+totalCount);

            foreach (var url in childmap) {
                var urls = url.Value;
                var index = 0;
                Console.WriteLine(_webHostEnvironment.ContentRootPath + "static/");
                var albumList = new List<string>();
                foreach (var url2 in urls) {
                    var fileName = url.Key + index++ + ".jpg";
                    try
                    {
                        //var test = await url2.WithHeaders(new
                        //{
                        //    Accept = @"image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8",
                        //    Host = @"img.xsnvshen.com",
                        //    Referer = @"https://www.xsnvshen.com/album/" + url.Key,
                        //}).DownloadFileAsync(_webHostEnvironment.ContentRootPath + "wwwroot/" + url.Key + "/", fileName);
                        
                    }
                    catch
                    {
                        Console.WriteLine(@"https://www.xsnvshen.com/album/" + url.Key);
                        NLogUtil.WriteAll(NLog.LogLevel.Error,LogType.Web,"ʧ��",$"��ȡͼƬʧ��https://www.xsnvshen.com/album/{url.Key}");
                        continue;
                    }
                    finally {
                        albumList.Add(fileName);
                    }
                    Console.WriteLine(count++);
                }
                await _redisOperationRepository.SortedSetAddAsync("img",JsonConvert.SerializeObject(albumList),url.Key.ObjectToDouble());
            }

            
        }
       /* [HttpGet]
        public async Task<IActionResult> showImg([FromQuery]int page=0) {
            var list = await getImgList(page);
            var item=list.FirstOrDefault();
            var strList=JsonConvert.DeserializeObject<string[]>(item.ToString());
            ViewBag.th = strList.FirstOrDefault().Substring(0, strList.FirstOrDefault().Length-5);
            ViewBag.page = page++;
            return View(strList);
        }*/
        [HttpGet]
        public async Task<IEnumerable<string>> getImgList([FromQuery] int page=0) {
            var listImg =await _redisOperationRepository.SortedSetRangeByRankAsync("img",page,page);
           return listImg;
        }
        [HttpGet]
        public async void test() {
            NLogUtil.WriteFileLog(NLog.LogLevel.Info,LogType.Order,"�ɹ��ص�","�ɹ�");
            await _humanInfoServices.InsertAsync(new Entity.HumanInfo { Name="wswj",Description="����",HumanId=12});
            //var csredis = new CSRedis.CSRedisClient("redistest:6379,defaultDatabase=13,prefix=my_");
            //csredis.Set("test",1);
            var client = _httpClientFactory.CreateClient("mn");
            var response = await client.GetAsync("/girl/22162");
            var s= await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            Console.WriteLine(s);
        }
        [HttpPost]
        public async Task<WebApiCallBack> AddImg([FromForm] FormFileCollection file)
        {
            HttpRequest request=_http.HttpContext.Request;
            Console.WriteLine(file.Count);
            return await _minIOServices.UploadImageAsync(file);
        }
    }
    }
