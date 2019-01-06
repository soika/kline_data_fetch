namespace WebApi.Objects
{
    using CryptoExchange.Net.Objects;

    /// <summary>
    ///     OkexSocketClientOptions class
    /// </summary>
    public class OkexSocketClientOptions : SocketClientOptions
    {
        public OkexSocketClientOptions()
        {
            BaseAddress = "wss://real.okex.com:10442/ws/v3/";
        }

        public string BaseSocketCombinedAddress { get; set; } = "wss://real.okex.com:10442/ws/v3/";

        public OkexSocketClientOptions Copy()
        {
            var copy = Copy<OkexSocketClientOptions>();
            copy.BaseSocketCombinedAddress = BaseSocketCombinedAddress;
            return copy;
        }
    }
}