namespace WebApi.Jobs.Bitfinex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Data.Collections;
    using global::Bitfinex.Net;
    using global::Bitfinex.Net.Objects;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class TimedBitfinexHostedService : TimedHostedService
    {
        private readonly BitfinexClient bitfinexClient;
        private readonly IBitfinexService bitfinexService;

        private readonly TimeFrame[] fetchIntervals = Enum.GetValues(typeof(TimeFrame))
                                                          .Cast<TimeFrame>()
                                                          .Where(x => x != TimeFrame.OneMinute)
                                                          .ToArray();

        private readonly IBackgroundTaskQueue taskQueue;

        public TimedBitfinexHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.bitfinexClient = this.ServiceProvider.GetService<BitfinexClient>();
            this.taskQueue = this.ServiceProvider.GetService<IBackgroundTaskQueue>();
            this.bitfinexService = this.ServiceProvider.GetService<IBitfinexService>();
        }

        protected override string ExpressionString => "*/6 * * * *";

        protected override async Task ProcessInScopeAsync()
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetService<IMapper>();
                var symbols = await GetBitfinexBtcBaseSymbols();

                if (!symbols.Any())
                {
                    return;
                }

                foreach (var symbol in symbols)
                {
                    foreach (var fetchInterval in this.fetchIntervals)
                    {
                        var fetchIntervalResult = await this.bitfinexClient.GetCandlesAsync(fetchInterval, $"t{symbol.ToUpper()}", 1);
                        this.taskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            var processedItems = fetchIntervalResult.Data.Select(x => mapper.Map<BitfinexCandle, BitfinexKlineDocument>(x)).ToArray();

                            foreach (var bitfinexKline in processedItems)
                            {
                                bitfinexKline.Symbol = symbol.ToUpper();
                                bitfinexKline.Expression = $"{bitfinexKline.Symbol}-{fetchInterval}";
                                bitfinexKline.KlineInterval = fetchInterval.ToString();
                            }

                            await this.bitfinexService.AddKlineArrayData(processedItems);
                        });
                    }
                }
            }
        }

        private async Task<IList<string>> GetBitfinexBtcBaseSymbols()
        {
            var symbolResult = await this.bitfinexClient.GetSymbolsAsync();
            var filteredSymbols = symbolResult.Data.Where(x => x.EndsWith("btc", StringComparison.OrdinalIgnoreCase));

            return filteredSymbols.ToList();
        }
    }
}