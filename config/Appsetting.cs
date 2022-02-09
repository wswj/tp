using Microsoft.Extensions.Options;

namespace FluUrl.config
{
    public class MinIOConfig : IOptions<MinIOConfig>
    {
        public MinIOConfig Value => this;
        public string BucketName { get; set; }=string.Empty;
        public string FileURL { get; set; } = string.Empty;
    }
}
