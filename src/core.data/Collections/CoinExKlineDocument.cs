namespace WebApi.Collections
{
    using Core.Data.Collections;
    using MongoDbGenericRepository.Attributes;

    [CollectionName("CoinExKline")]
    public class CoinExKlineDocument : BaseKlineDocument
    {
    }
}