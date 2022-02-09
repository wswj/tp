using FluUrl.config;
using FluUrl.Entity;
using FluUrl.Helper;
using FluUrl.Service.IService;
using FluUrl.ViewModels.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Minio;
using Minio.AspNetCore;
using System.Text.Json;

namespace FluUrl.Service
{
    public class MinIOServices : IMinIOServices
    {
        /// <summary>
        /// 上传文件相关
        /// </summary>

        public MinIOConfig _settings { get; }
        public IWebHostEnvironment _hostingEnvironment { get; set; }
        public MinioClient _client { get; set; }
        public MinioOptions _minioOptions { get; set; }
        public MinIOServices(IOptions<MinIOConfig> setting, IWebHostEnvironment hostingEnvironment, MinioClient client, IOptions<MinioOptions> minioOptions)
        {
            _settings = setting.Value;
            _hostingEnvironment = hostingEnvironment;
            _client = client;
            _minioOptions = minioOptions.Value;
        }
        //获取图片的返回类型
        public static Dictionary<string, string> contentTypDict = new Dictionary<string, string> {
               {"bmp","image/bmp" },
               {"jpg","image/jpeg"},
               {"jpeg","image/jpeg"},
               {"jpe","image/jpeg"},
               {"png","image/png"},
               {"gif","image/gif"},
               {"ico","image/x-ico"},
               {"tif","image/tiff"},
               {"tiff","image/tiff"},
               {"fax","image/fax"},
               {"wbmp","image//vnd.wap.wbmp"},
               {"rp","image/vnd.rn-realpix"} };


        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns></returns>

        public async Task<WebApiCallBack> UploadImageAsync(FormFileCollection file)
        {
            //Result<FileModel> res = new Result<FileModel>(false, "上传失败");
            WebApiCallBack res = new WebApiCallBack();
            //获得文件扩展名
            string fileNameEx = System.IO.Path.GetExtension(file[0].FileName).Replace(".", "");

            //是否是图片，现在只能是图片上传 文件类型 或扩展名不一致则返回
            if (contentTypDict.Values.FirstOrDefault(c => c == file[0].ContentType.ToLower()) == null || contentTypDict.Keys.FirstOrDefault(c => c == fileNameEx) == null)
            {
                res.msg = "图片格式不正确";
                return res;
            }
            else
                return await UploadAsync(file);
        }



        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns></returns>

        public async Task<WebApiCallBack> UploadAsync(FormFileCollection file)
        {
            //Result<FileModel> res = new Result<FileModel>(false, "上传失败");
            WebApiCallBack res = new WebApiCallBack();
            try
            {

                //存储桶名
                string bucketName = _settings.BucketName;

                FileModel fileModel = new FileModel();
                //如果桶名不存在则新建
                await CreateBucket(bucketName);
                //生成新的文件名
                var newFileName = CreateNewFileName(bucketName, file[0].FileName);
                //上传原图
                await _client.PutObjectAsync(bucketName, newFileName, file[0].OpenReadStream(), file[0].Length,
                    file[0].ContentType);
                fileModel.Url = $"{_settings.FileURL}{newFileName}";

                //判断刚才上传的文件是否是图片，如果是图片进行压缩,将压缩后的文件也上传
                /*if (contentTypDict.Values.Contains(file[0].ContentType.ToLower()))
                {
                    //新建目录用于保存
                    string path = $"{_hostingEnvironment.ContentRootPath}/wwwroot/imgTemp/";
                    //判断文件夹是否存在没有则新建
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                    var bImageName = $"m_{newFileName}";
                    var savepath = $"{path}m_{newFileName}";//保存绝对路径
                    #region 保存原图到本地
                    using (FileStream fs = System.IO.File.Create(path + bImageName))
                    {
                        file[0].CopyTo(fs);
                        fs.Flush();
                    }
                    #endregion

                    #region 保存缩略图到本地
                    //var bUrlRes = TencentCloudImageHelper.GetThumbnailImage(240, newFileName, path);
                    #endregion

                    //上传压缩图
                    //if (bUrlRes.Res)
                    //{
                    using (var sw = new FileStream(savepath, FileMode.Open))
                    {
                        await _client.PutObjectAsync(bucketName, bImageName, sw, sw.Length,
             "image/jpeg");
                        fileModel.BUrl = $"{_settings.FileURL}{bImageName}";
                    }
                    //删除本地的图片
                    if (Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.Delete(Path.GetDirectoryName(path), true);
                    //}
                }*/
                res.status = true; 
                res.msg = "上传成功";
                res.data = fileModel;
                return res;
            }
            catch (Exception e)
            {

                return res;
            }
        }

        public async Task<WebApiCallBack> UploadPdf(Stream file)
        {
            //Result<FileModel> res = new Result<FileModel>(false, "上传失败");
            WebApiCallBack res = new WebApiCallBack();
            try
            {

                //存储桶名
                string bucketName = _settings.BucketName;

                FileModel fileModel = new FileModel();
                await CreateBucket(bucketName);
                var newFileName = CreateNewFileName(bucketName, "授权书.pdf");
                await _client.PutObjectAsync(bucketName, newFileName, file, file.Length,
                    "application/pdf");
                fileModel.Url = $"{_settings.FileURL}{newFileName}";

                res.status = true;
                res.msg = "上传成功";
                res.data = fileModel;
                return res;
            }
            catch (Exception e)
            {

                return res;
            }
        }

        private async Task CreateBucket(string bucketName)
        {
            var found = await _client.BucketExistsAsync(bucketName);
            if (!found)
            {
                await _client.MakeBucketAsync(bucketName);
                //设置只读策略
                var pObj = new
                {
                    Version = "2012-10-17",
                    Statement = new[]
                    {
                       new
                       {
                           Effect = "Allow",
                           Principal = new
                           {
                               AWS = new [] {"*"}
                           },
                           Action = new [] {"s3:GetBucketLocation", "s3:ListBucket"},
                           Resource = new []
                           {
                               $"arn:aws:s3:::{bucketName}"
                           }
                       },
                       new
                       {
                           Effect = "Allow",
                           Principal = new
                           {
                               AWS = new [] {"*"}
                           },
                           Action = new [] {"s3:GetObject"},
                           Resource = new []
                           {
                               $"arn:aws:s3:::{bucketName}/*"
                           }
                       }
                   }
                };
                var po = JsonSerializer.Serialize(pObj);
                await _client.SetPolicyAsync(bucketName, po);
            }
        }
        /// <summary>
        /// 根据桶名和原有文件名生成新的文件名
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="oldFileName"></param>
        /// <returns></returns>
        private string CreateNewFileName(string bucketName, string oldFileName)
        {
            var dt = Guid.NewGuid().ToString().Replace("-", "").Substring(10) + DateTimeOffset.Now.ToUnixTimeSeconds();
            var extensions = Path.GetExtension(oldFileName);
            var newFileName = $"{bucketName}-{dt}{extensions}";
            return newFileName;
        }

    }
}
