namespace WebApi.Jobs.Huobi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Data.Collections;
    using global::Huobi.Net;
    using global::Huobi.Net.Objects;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class TimedHuobiHostedService : TimedHostedService
    {
        private readonly HuobiPeriod[] fetchIntervals = Enum.GetValues(typeof(HuobiPeriod))
                                                            .Cast<HuobiPeriod>()
                                                            .Where(x => x != HuobiPeriod.OneMinute
                                                                     && x != HuobiPeriod.OneYear)
                                                            .ToArray();

        private readonly HuobiClient huobiClient;
        private readonly IHuobiService huobiService;
        private readonly IBackgroundTaskQueue taskQueue;

        public TimedHuobiHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.huobiClient = this.ServiceProvider.GetService<HuobiClient>();
            this.taskQueue = this.ServiceProvider.GetService<IBackgroundTaskQueue>();
            this.huobiService = this.ServiceProvider.GetService<IHuobiService>();
        }

        protected override string ExpressionString => "*/6 * * * *";

        protected override async Task ProcessInScopeAsync()
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetService<IMapper>();

                var symbols = await GetHuobiBtcBaseSymbols();

                if (!symbols.Any())
                {
                    return;
                }

                foreach (var symbol in symbols)
                {
                    foreach (var fetchInterval in this.fetchIntervals)
                    {
                        // Get latest kline data
                        var fetchIntervalResult = await this.huobiClient.GetMarketKlinesAsync(symbol, fetchInterval, 1);
                        this.taskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            var processedItems = fetchIntervalResult.Data.Select(x => mapper.Map<HuobiMarketKline, HuobiKlineDocument>(x)).ToArray();

                            foreach (var huobiKline in processedItems)
                            {
                                huobiKline.Symbol = symbol.ToUpper();
                                huobiKline.Expression = $"{huobiKline.Symbol}-{fetchInterval}";
                                huobiKline.KlineInterval = fetchInterval.ToString();
                            }

                            await this.huobiService.AddKlineArrayData(processedItems);
                        });
                    }
                }
            }
        }

        private async Task<IList<string>> GetHuobiBtcBaseSymbols()
        {
            var symbolResult = await this.huobiClient.GetSymbolsAsync();
            var filteredSymbols = symbolResult.Data.Where(x => x.QuoteCurrency.Equals("btc", StringComparison.OrdinalIgnoreCase));

            return filteredSymbols.Select(x => x.Symbol).ToList();
        }
    }
}