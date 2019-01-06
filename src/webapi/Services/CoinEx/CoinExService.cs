namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Collections;
    using Config;
    using Core.Data.Repositories;
    using Microsoft.Extensions.Options;

    public class CoinExService : ICoinExService
    {
        private readonly AppMongoRepository repository;

        public CoinExService(IOptions<MongoServerSettings> options)
        {
            this.repository = new AppMongoRepository(options.Value.ConnectionString, options.Value.Database);
        }

        public async Task AddKlineData(CoinExKlineDocument data)
        {
            var existedItem = await this.repository.GetOneAsync<CoinExKlineDocument>(x => x.Expression.Equals($"{data.Symbol}-{data.KlineInterval}"));
            if (existedItem == null)
            {
                await this.repository.AddOneAsync(data);
                return;
            }

            await this.repository.UpdateOneAsync(data);
        }

        public async Task AddKlineArrayData(CoinExKlineDocument[] array)
        {
            foreach (var coinExKline in array)
            {
                await AddKlineData(coinExKline);
            }
        }
    }
}