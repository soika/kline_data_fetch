namespace Core.Data.Collections
{
    using System;
    using MongoDbGenericRepository.Models;

    public abstract class BaseKlineDocument : Document
    {
        public DateTime Timestamp { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal Low { get; set; }

        public decimal High { get; set; }

        public decimal Volume { get; set; }

        public string Symbol { get; set; }

        public string KlineInterval { get; set; }

        public string Expression { get; set; }
    }
}