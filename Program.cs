using FluUrl.config;
using FluUrl.Helper;
using FluUrl.Utils;
using JasonSoft.Net.JsHttpClient.Extensions;
using StackExchange.Redis;
using System.Net;
using NLog;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using FluUrl.AutoFac;
using Minio.AspNetCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:443");
builder.WebHost.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
});
builder.Services.AddSingleton(new AppSettingsHelper(builder.Environment.ContentRootPath));
//添加jshttp获得html内容
builder.Services.AddJsHttpClient(new JsHttpClientOptions { AllowAutoRedirect = false });
//添加httpclient
builder.Services.AddHttpClient("mn", c =>
                {
                    c.BaseAddress = new Uri("https://www.xsnvshen.com");
                    c.DefaultRequestHeaders.Add("Accept", "text/javascript, application/javascript, application/ecmascript, application/x-ecmascript, */*; q=0.01");
                    c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36");
                    c.DefaultRequestHeaders.Add("Host", "www.xsnvshen.com");
                    c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    //c.DefaultRequestHeaders.Add("Cookie", "__51vcke__JNmlfXHHIrHMZgLq=a0b8f008-993e-5177-8bba-c9f00a8ce164; __51vuft__JNmlfXHHIrHMZgLq=1642565015032; __51uvsct__JNmlfXHHIrHMZgLq=3; __test; __PPU___PPU_SESSION_URL=%2F; jpx=49; __vtins__JNmlfXHHIrHMZgLq=%7B%22sid%22%3A%20%22d37ccc22-22fe-5a0f-902b-b6d04161833f%22%2C%20%22vd%22%3A%2010%2C%20%22stt%22%3A%201412808%2C%20%22dr%22%3A%20222644%2C%20%22expires%22%3A%201642577824650%2C%20%22ct%22%3A%201642576024650%7D");
                    c.DefaultRequestHeaders.Add("Referer", "https://www.xsnvshen.com/girl/22162");
                })
                .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                {
                    

                    var handler = new HttpClientHandler();
                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }
                    return handler;
                });
builder.Services.AddCors(policy =>
{
    policy.AddPolicy("CorsPolicy", opt => opt
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});
//添加redis支持
builder.Services.AddRedisCacheSetup();
builder.Services.AddSqlSugarSetup();
builder.Host
.UseServiceProviderFactory(new AutofacServiceProviderFactory())
.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new AutofacModuleRegister());
});
builder.Services.AddMinio(
    options => {
        options.Endpoint = builder.Configuration.GetSection("Minio")["Endpoint"];  // 这里是在配置文件接口访问域名
        options.Region = builder.Configuration.GetSection("Minio")["Region"];  // 地址
        options.AccessKey = builder.Configuration.GetSection("Minio")["AccessKey"];  // 用户名
        options.SecretKey = builder.Configuration.GetSection("Minio")["SecretKey"];   // 密码
    }
    );
//注册自定义MinIO配置文件
builder.Services.Configure<MinIOConfig>(builder.Configuration.GetSection(nameof(MinIOConfig)));

//注入httcontext
builder.Services.AddHttpContextAccessor();
// Add services to the container.
//添加redis连接池  暂时不使用
//var redisConfig=builder.Configuration.GetSection("Redis").Get<RedisConfig>();
//var redisConfigurationOption =new ConfigurationOptions {
//    EndPoints = { { redisConfig.Host, redisConfig.Port } },
//    Password = redisConfig.Password
//};
//builder.Services.AddRedisConnectionPool(redisConfigurationOption,redisConfig.PoolSize);


builder.Services.AddControllers();
builder.Services.AddMvc();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("CorsPolicy");
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();
