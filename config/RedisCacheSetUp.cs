using FluUrl.Helper;
using FluUrl.Repository;
using FluUrl.Repository.IRepository;
using StackExchange.Redis;

namespace FluUrl.config
{
    public static class RedisCacheSetUp
    {
        public static void AddRedisCacheSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IRedisOperationRepository, RedisOperationRepository>();

            // 配置启动Redis服务，虽然可能影响项目启动速度，但是不能在运行的时候报错，所以是合理的
            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                //获取连接字符串
                string redisConfiguration = AppSettingsHelper.GetContent("RedisConfig", "ConnectionString");

                var configuration = ConfigurationOptions.Parse(redisConfiguration, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });

        }
    }
}
//https://vdownload-1.sb-cd.com/5/4/5460161-720p.mp4?secure=NxYt9aS6CrlekUp6iBE8IQ,1642876332&m=1&d=3&_tid=5460161