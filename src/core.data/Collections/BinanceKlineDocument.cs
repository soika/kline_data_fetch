namespace Core.Data.Collections
{
    using System;
    using MongoDbGenericRepository.Attributes;

    [CollectionName("BinanceKline")]
    public class BinanceKlineDocument : BaseKlineDocument
    {
        public DateTime OpenTime { get; set; }

        public DateTime CloseTime { get; set; }
    }
}