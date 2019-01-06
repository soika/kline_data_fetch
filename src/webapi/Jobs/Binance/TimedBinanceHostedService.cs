namespace WebApi.Jobs.Binance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Data.Collections;
    using global::Binance.Net;
    using global::Binance.Net.Objects;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class TimedBinanceHostedService : TimedHostedService
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly BinanceClient binanceClient;
        private readonly IBinanceService binanceService;

        private readonly KlineInterval[] fetchIntervals = Enum.GetValues(typeof(KlineInterval))
                                                              .Cast<KlineInterval>()
                                                              .Where(x => x != KlineInterval.OneMinute)
                                                              .ToArray();

        public TimedBinanceHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.binanceClient = serviceProvider.GetService<BinanceClient>();
            this.taskQueue = serviceProvider.GetService<IBackgroundTaskQueue>();
            this.binanceService = serviceProvider.GetService<IBinanceService>();
        }

        protected override string ExpressionString => "*/2 * * * *";

        protected override async Task ProcessInScopeAsync()
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetService<IMapper>();

                var symbols = await GetBinanceBtcBaseSymbols();

                if (!symbols.Any())
                {
                    return;
                }

                foreach (var symbol in symbols)
                {
                    foreach (var fetchInterval in this.fetchIntervals)
                    {
                        // Get latest kline data
                        var fetchIntervalResult = await this.binanceClient.GetKlinesAsync(symbol, fetchInterval, null, null, 1);

                        this.taskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            var processedItems = fetchIntervalResult.Data.Select(x => mapper.Map<BinanceKline, BinanceKlineDocument>(x)).ToArray();

                            foreach (var binanceKline in processedItems)
                            {
                                binanceKline.Symbol = symbol;
                                binanceKline.Expression = $"{binanceKline.Symbol}-{fetchInterval}";
                                binanceKline.KlineInterval = fetchInterval.ToString();
                            }

                            await this.binanceService.AddKlineArrayData(processedItems);
                        });
                    }
                }
            }
        }

        private async Task<IList<string>> GetBinanceBtcBaseSymbols()
        {
            var exchangeInfo = await this.binanceClient.GetExchangeInfoAsync();
            var filteredSymbols = exchangeInfo.Data.Symbols
                                              .Where(x => x.QuoteAsset.Equals("btc", StringComparison.OrdinalIgnoreCase))
                                              .Select(x => x.Name);

            return filteredSymbols.ToList();
        }
    }
}