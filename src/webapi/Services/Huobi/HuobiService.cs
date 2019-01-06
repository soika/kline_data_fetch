namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Config;
    using Core.Data.Collections;
    using Core.Data.Repositories;
    using Microsoft.Extensions.Options;

    public class HuobiService : IHuobiService
    {
        private readonly AppMongoRepository repository;

        public HuobiService(IOptions<MongoServerSettings> options)
        {
            this.repository = new AppMongoRepository(options.Value.ConnectionString, options.Value.Database);
        }


        public async Task AddKlineData(HuobiKlineDocument data)
        {
            var existedItem = await this.repository.GetOneAsync<HuobiKlineDocument>(x => x.Timestamp == data.Timestamp
                                                                                      && x.Expression.Equals($"{data.Symbol}-{data.KlineInterval}"));
            if (existedItem == null)
            {
                await this.repository.AddOneAsync(data);
                return;
            }

            await this.repository.UpdateOneAsync(data);
        }

        public async Task AddKlineArrayData(HuobiKlineDocument[] array)
        {
            foreach (var huobiKline in array)
            {
                await AddKlineData(huobiKline);
            }
        }
    }
}