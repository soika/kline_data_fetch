namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Config;
    using Core.Data.Collections;
    using Core.Data.Repositories;
    using Microsoft.Extensions.Options;

    public class BittrexService : IBittrexService
    {
        private readonly AppMongoRepository repository;

        public BittrexService(IOptions<MongoServerSettings> options)
        {
            this.repository = new AppMongoRepository(options.Value.ConnectionString, options.Value.Database);
        }


        public async Task AddKlineData(BittrexKlineDocument data)
        {
            var existedItem = await this.repository.GetOneAsync<BittrexKlineDocument>(x => x.Expression.Equals($"{data.Symbol}-{data.KlineInterval}"));
            if (existedItem == null)
            {
                await this.repository.AddOneAsync(data);
                return;
            }

            await this.repository.UpdateOneAsync(data);
        }

        public async Task AddKlineArrayData(BittrexKlineDocument[] array)
        {
            foreach (var bittrexKline in array)
            {
                await AddKlineData(bittrexKline);
            }
        }
    }
}