namespace WebApi.Objects.Okex
{
    using Newtonsoft.Json;

    public class OkexStreamKlineData
    {
        [JsonProperty("table")]
        public string Table { get; set; }

        [JsonProperty("data")]
        public OkexStreamKline Data { get; set; }
    }

    public class OkexStreamKline
    {
        [JsonProperty("instrument_id")]
        public string InstrumentId { get; set; }

        [JsonProperty("candle")]
        public string[] Candle { get; set; }
    }
}