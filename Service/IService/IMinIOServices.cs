using FluUrl.ViewModels.UI;

namespace FluUrl.Service.IService
{
    public interface IMinIOServices
    {

            /// <summary>
            /// 上传
            /// </summary>
            /// <param name="file">文件</param>
            /// <returns></returns>
            Task<WebApiCallBack> UploadAsync(FormFileCollection file);

            /// <summary>
            /// 上传图片
            /// </summary>
            /// <param name="file">文件</param>
            /// <returns></returns>

            Task<WebApiCallBack> UploadImageAsync(FormFileCollection file);

            /// <summary>
            /// 上传pdf
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            Task<WebApiCallBack> UploadPdf(Stream file);
        }
    
}
