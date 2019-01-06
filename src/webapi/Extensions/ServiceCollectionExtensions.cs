namespace WebApi.Extensions
{
    using AutoMapper;
    using Binance.Net;
    using Bitfinex.Net;
    using Bittrex.Net;
    using CoinEx.Net;
    using Config;
    using Core.Data;
    using Huobi.Net;
    using Jobs;
    using Jobs.Binance;
    using Jobs.Bitfinex;
    using Jobs.Bittrex;
    using Jobs.Huobi;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using WebSocketClients;

    public static class ServiceCollectionExtensions
    {
        public static void RegisterBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<QueuedHostedService>();
            // services.AddHostedService<TimedCoinExHostedService>();
            //services.AddHostedService<TimedHuobiHostedService>();
            //services.AddHostedService<TimedBittrexHostedService>();
            //services.AddHostedService<TimedBitfinexHostedService>();
            services.AddHostedService<TimedBinanceHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }

        public static void RegisterDbContext(this IServiceCollection services,
                                             IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                                                            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        }

        public static void RegisterSocketClients(this IServiceCollection services)
        {
            services.AddTransient(typeof(ExtendedBinanceWebSocketClient));
            services.AddTransient(typeof(BinanceClient));
        }

        public static void RegisterApiClients(this IServiceCollection services)
        {
            services.AddTransient(typeof(CoinExClient));
            services.AddTransient(typeof(HuobiClient));
            services.AddTransient(typeof(BittrexClient));
            services.AddTransient(typeof(BitfinexClient));
            services.AddTransient(typeof(BinanceClient));
        }

        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<ICoinExService, CoinExService>();
            services.AddTransient<IBittrexService, BittrexService>();
            services.AddTransient<IBitfinexService, BitfinexService>();
            services.AddTransient<IBinanceService, BinanceService>();
            services.AddTransient<IHuobiService, HuobiService>();
        }

        public static void RegisterMappers(this IServiceCollection services)
        {
            services.AddAutoMapper();
        }

        public static void MapAllConfigurationClasses(this IServiceCollection services,
                                                      IConfiguration configuration)
        {
            services.Configure<MongoServerSettings>(configuration.GetSection("MongoServerSettings"));
        }
    }
}