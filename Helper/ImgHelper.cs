using System.Drawing;
using System.Drawing.Imaging;

namespace FluUrl.Helper
{
    public class ImgHelper
    {
        /// <summary>
        /// 指定缩放类型
        /// </summary>
        public enum ImgThumbnailType
        {
            /// <summary>
            /// 无
            /// </summary>
            Nothing = 0,


            /// <summary>
            /// 指定高宽缩放（可能变形）
            /// </summary>
            WH = 1,


            /// <summary>
            /// 指定宽，高按比例
            /// </summary>
            W = 2,


            /// <summary>
            /// 指定高，宽按比例
            /// </summary>
            H = 3,


            /// <summary>
            /// 指定高宽裁减（不变形）
            /// </summary>
            Cut = 4,


            /// <summary>
            /// 按照宽度成比例缩放后，按照指定的高度进行裁剪
            /// </summary>
            W_HCut = 5,

            /// <summary>
            /// 长边优先
            /// </summary>
            W_L = 5,

            /// <summary>
            /// 短边优先
            /// </summary>
            W_S = 5,
        }
        /// <summary>
        /// 指定优先边
        /// </summary>
        public enum ImgResize
        {
            /// <summary>
            /// 无
            /// </summary>
            Nothing = 0,

            /// <summary>
            /// 按长边优先
            /// </summary>
            M_lfit = 1,

            /// <summary>
            /// 按短边优先
            /// </summary>
            M_mfit = 2,
        }
        //将图片转换为base64字符串
        public static string ImageToBase64(string filePath) {
            Bitmap bmp = new Bitmap(filePath);
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms,bmp.RawFormat);
            byte[] data = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(data,0,data.Length);
            ms.Close();
            return Convert.ToBase64String(data);
        }
        public static string GetImg(string imgstr,int type,int length=1024*1536) {
            if (imgstr.Length<length) {
                return imgstr;
            }
            int width = 800;
            int height = 560;
            if (type==1) {
                width = 1230;
                height = 870;
            }
            byte[] imgBytes=Convert.FromBase64String(imgstr);
            var stream=new MemoryStream(imgBytes);
            var image=Image.FromStream(stream);
            double newWidth,newHeight;
            //如果图片宽度大于高度按宽度压缩,否则相反
            if (image.Width > image.Height)
            {
                newWidth = width;
                newHeight = image.Height * (newWidth / image.Width);
            }
            else {
                newHeight = height;
                newWidth = (newHeight / image.Height) * image.Width;
            }
            if (newWidth>width) {
                newWidth=width;
            }
            if (newHeight>height) {
                newHeight = height;
                newWidth = image.Width * (newHeight / image.Height);
            }
            var outStream=new MemoryStream();
            Thumbnail(stream,outStream,Convert.ToInt32(newWidth),Convert.ToInt32(newHeight), 100, ImgThumbnailType.WH);
            var newImageStr=Convert.ToBase64String(outStream.ToArray());
            while (newImageStr.Length>=length) {
                newImageStr = GetImg(newImageStr,type,length);

            }
            return newImageStr;
        }
        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sourceStream">原图片</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="quality">压缩质量 1-100</param>
        /// <param name="type">压缩缩放类型</param>
        /// <param name="tFormat"></param>
        /// <returns></returns>
        private static Bitmap Thumbnail(Stream sourceStream, int width, int height, int quality
         , ImgThumbnailType type, out ImageFormat tFormat)
        {
            using (Image iSource=Image.FromStream(sourceStream)) {
                tFormat = iSource.RawFormat;
                //缩放后的宽度和高度
                int toWidth = width;
                int toHeight = height;

                int x = 0;
                int y = 0;
                //原始图片的高度和宽度
                int oWidth=iSource.Width
                    , oHeight=iSource.Height;
                //如果传入的类型为长边优先,判断图片的长短边
                if (type==ImgThumbnailType.W_L) {
                    type=oWidth>oHeight ? ImgThumbnailType.W : ImgThumbnailType.H;
                }
                //同上如果为短边优先
                if (type==ImgThumbnailType.W_S) {
                    type = oWidth > oHeight ? ImgThumbnailType.H : ImgThumbnailType.W;
                }
                switch (type)
                {
                    case ImgThumbnailType.Nothing:
                        break;
                    case ImgThumbnailType.WH://指定高宽缩放（可能变形）
                        break;
                    case ImgThumbnailType.W://指定宽，高按比例 
                        {
                            toHeight = iSource.Height * width / iSource.Width;
                            break;
                        }
                    case ImgThumbnailType.H://指定高，宽按比例
                        {
                            toWidth = iSource.Width * height / iSource.Height;
                            break;
                        }
                    case ImgThumbnailType.Cut://指定高宽裁减（不变形）
                        {
                            if (iSource.Width / (double)iSource.Height > toWidth / (double)toHeight)
                            {
                                oHeight = iSource.Height;
                                oWidth = iSource.Height * toWidth / toHeight;
                                y = 0;
                                x = (iSource.Width - oWidth) / 2;
                            }
                            else {
                                oWidth=iSource.Width;
                                oHeight=iSource.Width*height/toWidth;
                                x = 0;
                                y= (iSource.Height - oHeight) / 2;
                            }
                            break;
                        }
                    case ImgThumbnailType.W_HCut://按照宽度成比例缩放后，按照指定的高度进行裁剪
                        {
                            toHeight = iSource.Height * width / iSource.Width;
                            if (height < toHeight)
                            {
                                oHeight = oHeight * height / toHeight;
                                toHeight = toHeight * height / toHeight;
                            }
                            break;
                        }
                    default:
                        break;
                }
                Bitmap ob=new Bitmap(toWidth, toHeight);
                Graphics g= Graphics.FromImage(ob);
                g.Clear(Color.WhiteSmoke);
                g.CompositingQuality=System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(iSource,new Rectangle(x,y,toWidth,toHeight),new Rectangle(0,0,oWidth,oHeight),GraphicsUnit.Pixel);
                g.Dispose();
                return ob;
            }        
        }
        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sourceStream">原图片文件流</param>
        /// <param name="outStream">压缩后保存到流中</param>
        /// <param name="height">高度</param>
        /// <param name="width"></param>
        /// <param name="quality">压缩质量 1-100</param>
        /// <param name="type">压缩缩放类型</param>
        /// <returns></returns>
        public static bool Thumbnail(Stream sourceStream, Stream outStream, int width, int height, int quality, ImgThumbnailType type)
        {
            ImageFormat tFormat;
            Bitmap ob = Thumbnail(sourceStream, width, height, quality, type, out tFormat);
            //水印
            //ImgWaterMark.AddWaterMark(ob, "www.***.com");
            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1] { quality };//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int i = 0; i < arrayICI.Length; i++)
                {
                    if (arrayICI[i].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[i];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(outStream, jpegICIinfo, ep);//jpegICIinfo是压缩后的新路径
                }
                else
                {
                    ob.Save(outStream, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //iSource.Dispose();

                ob.Dispose();

            }
        }

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sourceFile">原图片</param>
        /// <param name="targetFile">压缩后保存位置</param>
        /// <param name="height">高度</param>
        /// <param name="width"></param>
        /// <param name="quality">压缩质量 1-100</param>
        /// <param name="type">压缩缩放类型</param>
        /// <returns></returns>
        public static bool Thumbnail(string sourceFile, string targetFile, int width, int height, int quality, ImgThumbnailType type)
        {
            ImageFormat tFormat = null;
            var fileBytes = File.ReadAllBytes(sourceFile);
            var sourceStream = new MemoryStream(fileBytes);
            Bitmap ob = Thumbnail(sourceStream, width, height, quality, type, out tFormat);
            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1] { quality };//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int i = 0; i < arrayICI.Length; i++)
                {
                    if (arrayICI[i].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[i];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(targetFile, jpegICIinfo, ep);//jpegICIinfo是压缩后的新路径
                }
                else
                {
                    ob.Save(targetFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //iSource.Dispose();

                ob.Dispose();

            }
        }



        #region 生成缩略图

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="sourceFile">原始图片文件</param>
        /// <param name="quality">质量压缩比</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="outputFile">输出文件名</param>
        /// <returns>成功返回true,失败则返回false</returns>
        public static bool GetThumImage(string sourceFile, long quality, int w, int h, string outputFile)
        {
            try
            {
                long imageQuality = quality;
                Bitmap sourceImage = new Bitmap(sourceFile);
                ImageCodecInfo myImageCodecInfo = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, imageQuality);
                myEncoderParameters.Param[0] = myEncoderParameter;
                sourceImage.Save(outputFile, myImageCodecInfo, myEncoderParameters);


                float xWidth = sourceImage.Width;
                float yWidth = sourceImage.Height;
                Bitmap newImage = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(newImage);

                g.DrawImage(sourceImage, 0, 0, w, h);
                g.Dispose();
                newImage.Save(outputFile, myImageCodecInfo, myEncoderParameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        #endregion


        public static ImgThumbnailType GetThumbnailType(string w, string h, string t, string sourceFile)
        {
            //参数只有一个
            if (w == "0" || h == "0")
            {
                return w == "0" ? ImgThumbnailType.H : ImgThumbnailType.W;
            }
            else
            {
                return t == "0" ? ImgThumbnailType.W_L : ImgThumbnailType.W_S;
            }
        }

        /// <summary>
        /// 比较原图的高宽大小
        /// </summary>
        /// <param name="w">宽度</param>
        /// <param name="h">高度</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int GetImgResize(int w, int h, int t)
        {
            if (w > h) return t == 0 ? w : h;
            return t == 0 ? h : w;
        }
    }
}

