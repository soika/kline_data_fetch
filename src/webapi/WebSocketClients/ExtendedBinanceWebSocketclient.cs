namespace WebApi.WebSocketClients
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Binance.Net;
    using Binance.Net.Converters;
    using Binance.Net.Objects;
    using CryptoExchange.Net.Logging;
    using CryptoExchange.Net.Objects;
    using CryptoExchange.Net.Sockets;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ExtendedBinanceWebSocketClient : BinanceSocketClient
    {
        private static readonly BinanceSocketClientOptions defaultOptions = new BinanceSocketClientOptions();
        private string baseCombinedAddress;

        public ExtendedBinanceWebSocketClient()
            : this(DefaultOptions)
        {
        }

        public ExtendedBinanceWebSocketClient(BinanceSocketClientOptions options)
            : base(options)
        {
            Configure(options);
        }

        private static BinanceSocketClientOptions DefaultOptions => defaultOptions.Copy();

        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineStreamAsync(string[] symbols,
                                                                                      KlineInterval[] intervals,
                                                                                      Action<BinanceStreamKlineData> onMessage)
        {
            void Handler(BinanceCombinedStream<BinanceStreamKlineData> data)
            {
                onMessage(data.Data);
            }

            var symbolList = new List<string>();

            foreach (var symbol in symbols)
            {
                foreach (var klineInterval in intervals)
                {
                    symbolList.Add($"{symbol.ToLower()}@kline_{JsonConvert.SerializeObject(klineInterval, (JsonConverter) new KlineIntervalConverter(false))}");
                }
            }

            var callResult = await Subscribe(string.Join("/", symbolList), true, (Action<BinanceCombinedStream<BinanceStreamKlineData>>) Handler).ConfigureAwait(false);
            return callResult;
        }

        public CallResult<UpdateSubscription> SubscribeToKlineStream(string[] symbols,
                                                                     KlineInterval[] intervals,
                                                                     Action<BinanceStreamKlineData> onMessage)
        {
            return SubscribeToKlineStreamAsync(symbols, intervals, onMessage).GetAwaiter().GetResult();
        }

        private async Task<CallResult<UpdateSubscription>> Subscribe<T>(string url,
                                                                        bool combined,
                                                                        Action<T> onData)
        {
            url = !combined ? BaseAddress + url : this.baseCombinedAddress + "stream?streams=" + url;
            var callResult = await CreateAndConnectSocket(url, onData).ConfigureAwait(false);
            var connectResult = callResult;
            callResult = null;
            return !connectResult.Success ? new CallResult<UpdateSubscription>(null, connectResult.Error) : new CallResult<UpdateSubscription>(new UpdateSubscription(connectResult.Data), null);
        }

        private async Task<CallResult<SocketSubscription>> CreateAndConnectSocket<T>(string url,
                                                                                     Action<T> onMessage)
        {
            var socket = CreateSocket(url);
            var subscription = new SocketSubscription(socket);
            subscription.MessageHandlers.Add("DataHandler", (subs,
                                                             data) => DataHandler(data, onMessage));
            var callResult = await ConnectSocket(subscription).ConfigureAwait(false);
            var connectResult = callResult;
            if (!connectResult.Success)
            {
                return new CallResult<SocketSubscription>(null, connectResult.Error);
            }

            socket.ShouldReconnect = true;
            return new CallResult<SocketSubscription>(subscription, null);
        }

        private bool DataHandler<T>(JToken data,
                                    Action<T> handler)
        {
            if (typeof(T) == typeof(string))
            {
                handler((T) Convert.ChangeType(data.ToString(), typeof(T)));
                return true;
            }

            var callResult = Deserialize<T>(data, false, null);
            if (!callResult.Success)
            {
                this.log.Write(LogVerbosity.Info, string.Format("Couldn't deserialize data received from stream of type {0}: ", typeof(T)) + callResult.Error);
                return false;
            }

            handler(callResult.Data);
            return true;
        }

        private void Configure(BinanceSocketClientOptions options)
        {
            this.baseCombinedAddress = options.BaseSocketCombinedAddress;
        }

        protected override bool SocketReconnect(SocketSubscription subscription,
                                                TimeSpan disconnectedTime)
        {
            return true;
        }
    }
}