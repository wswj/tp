namespace FluUrl.ViewModels.UI
{
    public class WebApiCallBack
    {
        /// <summary>
        ///     请求接口返回说明
        /// </summary>
        public string methodDescription { get; set; }


        /// <summary>
        ///     提交数据
        /// </summary>
        public object otherData { get; set; } = null;

        /// <summary>
        ///     状态码
        /// </summary>
        public bool status { get; set; } = false;

        /// <summary>
        ///     信息说明。
        /// </summary>
        public string msg { get; set; } = "接口响应成功";

        /// <summary>
        ///     返回数据
        /// </summary>
        public object data { get; set; }

        /// <summary>
        ///     返回编码
        /// </summary>
        public int code { get; set; } = 0;
    }
}
