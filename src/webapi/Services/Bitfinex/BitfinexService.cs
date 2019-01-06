namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Config;
    using Core.Data.Collections;
    using Core.Data.Repositories;
    using Microsoft.Extensions.Options;

    public class BitfinexService : IBitfinexService
    {
        private readonly AppMongoRepository repository;

        public BitfinexService(IOptions<MongoServerSettings> options)
        {
            this.repository = new AppMongoRepository(options.Value.ConnectionString, options.Value.Database);
        }


        public async Task AddKlineData(BitfinexKlineDocument data)
        {
            var existedItem = await this.repository.GetOneAsync<BitfinexKlineDocument>(x => x.Timestamp == data.Timestamp
                                                                                         && x.Expression.Equals($"{data.Symbol}-{data.KlineInterval}"));
            if (existedItem == null)
            {
                await this.repository.AddOneAsync(data);
                return;
            }

            await this.repository.UpdateOneAsync(data);
        }

        public async Task AddKlineArrayData(BitfinexKlineDocument[] array)
        {
            foreach (var bitfinexKline in array)
            {
                await AddKlineData(bitfinexKline);
            }
        }
    }
}