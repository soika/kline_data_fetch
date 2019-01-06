namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Config;
    using Core.Data.Collections;
    using Core.Data.Repositories;
    using Microsoft.Extensions.Options;

    public class BinanceService : IBinanceService
    {
        private readonly AppMongoRepository repository;

        public BinanceService(IOptions<MongoServerSettings> options)
        {
            this.repository = new AppMongoRepository(options.Value.ConnectionString, options.Value.Database);
        }


        public async Task AddKlineData(BinanceKlineDocument data)
        {
            var existedItem = await this.repository.GetOneAsync<BinanceKlineDocument>(x => x.CloseTime == data.CloseTime
                                                                                        && x.Expression.Equals($"{data.Symbol}-{data.KlineInterval}"));
            if (existedItem == null)
            {
                await this.repository.AddOneAsync(data);
                return;
            }

            await this.repository.UpdateOneAsync(data);
        }

        public async Task AddKlineArrayData(BinanceKlineDocument[] array)
        {
            foreach (var binanceKline in array)
            {
                await AddKlineData(binanceKline);
            }
        }
    }
}