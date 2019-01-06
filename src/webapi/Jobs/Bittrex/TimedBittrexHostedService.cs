namespace WebApi.Jobs.Bittrex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Data.Collections;
    using global::Bittrex.Net;
    using global::Bittrex.Net.Objects;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class TimedBittrexHostedService : TimedHostedService
    {
        private readonly BittrexClient bittrexClient;
        private readonly IBittrexService bittrexService;

        private readonly TickInterval[] fetchIntervals = Enum.GetValues(typeof(TickInterval))
                                                             .Cast<TickInterval>()
                                                             .Where(x => x != TickInterval.OneMinute)
                                                             .ToArray();

        private readonly IBackgroundTaskQueue taskQueue;


        public TimedBittrexHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.bittrexClient = this.ServiceProvider.GetService<BittrexClient>();
            this.taskQueue = this.ServiceProvider.GetService<IBackgroundTaskQueue>();
            this.bittrexService = this.ServiceProvider.GetService<IBittrexService>();
        }

        protected override string ExpressionString => "*/2 * * * *";

        protected override async Task ProcessInScopeAsync()
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetService<IMapper>();
                var symbols = await GetBittrexBtcBaseSymbols();
                if (!symbols.Any())
                {
                    return;
                }

                foreach (var symbol in symbols)
                {
                    foreach (var fetchInterval in this.fetchIntervals)
                    {
                        var fetchIntervalResult = await this.bittrexClient.GetCandlesAsync(symbol, fetchInterval);
                        this.taskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            var processedItems = fetchIntervalResult.Data.Select(x => mapper.Map<BittrexCandle, BittrexKlineDocument>(x)).ToArray();

                            foreach (var bittrexKline in processedItems)
                            {
                                bittrexKline.Symbol = symbol.ToUpper();
                                bittrexKline.Expression = $"{bittrexKline.Symbol}-{fetchInterval}";
                                bittrexKline.KlineInterval = fetchInterval.ToString();
                            }

                            await this.bittrexService.AddKlineArrayData(processedItems);
                        });
                    }
                }
            }
        }

        private async Task<IList<string>> GetBittrexBtcBaseSymbols()
        {
            var marketSummariesResult = await this.bittrexClient.GetMarketSummariesAsync();
            var filteredSymbols = marketSummariesResult.Data.Where(x => x.MarketName.EndsWith("btc", StringComparison.OrdinalIgnoreCase));

            return filteredSymbols.Select(x => x.MarketName).ToList();
        }
    }
}