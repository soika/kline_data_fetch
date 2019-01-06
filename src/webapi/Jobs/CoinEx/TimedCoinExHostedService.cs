namespace WebApi.Jobs.CoinEx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Collections;
    using global::CoinEx.Net;
    using global::CoinEx.Net.Objects;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class TimedCoinExHostedService : TimedHostedService
    {
        private readonly CoinExClient coinExClient;
        private readonly ICoinExService coinExService;

        private readonly KlineInterval[] fetchIntervals = Enum.GetValues(typeof(KlineInterval))
                                                              .Cast<KlineInterval>()
                                                              .Where(x => x != KlineInterval.OneMinute)
                                                              .ToArray();

        private readonly IBackgroundTaskQueue taskQueue;

        public TimedCoinExHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.coinExClient = this.ServiceProvider.GetService<CoinExClient>();
            this.taskQueue = this.ServiceProvider.GetService<IBackgroundTaskQueue>();
            this.coinExService = this.ServiceProvider.GetService<ICoinExService>();
        }

        protected override string ExpressionString => "*/6 * * * *";

        protected override async Task ProcessInScopeAsync()
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetService<IMapper>();
                var symbols = await GetCoinExBtcBaseSymbols();
                if (!symbols.Any())
                {
                    return;
                }

                foreach (var symbol in symbols)
                {
                    foreach (var fetchInterval in this.fetchIntervals)
                    {
                        var fetchIntervalResult = await this.coinExClient.GetKlinesAsync(symbol, fetchInterval, 1);

                        this.taskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            var processedItems = fetchIntervalResult.Data.Select(x => mapper.Map<CoinExKline, CoinExKlineDocument>(x)).ToArray();

                            foreach (var coinExKline in processedItems)
                            {
                                coinExKline.Expression = $"{coinExKline.Symbol}-{fetchInterval}";
                                coinExKline.KlineInterval = fetchInterval.ToString();
                            }

                            await this.coinExService.AddKlineArrayData(processedItems);
                        });
                    }
                }
            }
        }

        private async Task<IList<string>> GetCoinExBtcBaseSymbols()
        {
            var symbolResult = await this.coinExClient.GetMarketListAsync();
            var filteredSymbols = symbolResult.Data.Where(x => x.EndsWith("btc", StringComparison.OrdinalIgnoreCase));

            return filteredSymbols.ToList();
        }
    }
}