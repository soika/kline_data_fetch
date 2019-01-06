namespace WebApi.WebSocketClients
{
    using System;
    using System.Threading.Tasks;
    using CryptoExchange.Net;
    using CryptoExchange.Net.Logging;
    using CryptoExchange.Net.Objects;
    using CryptoExchange.Net.Sockets;
    using Newtonsoft.Json.Linq;
    using Objects;
    using Objects.Okex;
    using KlineInterval = Objects.KlineInterval;

    public class OkexWebSocketClient : SocketClient, IOkexWebSocketClient
    {
        private static readonly OkexSocketClientOptions defaultOptions = new OkexSocketClientOptions();

        private string baseCombinedAddress;

        public OkexWebSocketClient() : this(DefaultOptions)
        {
        }

        public OkexWebSocketClient(OkexSocketClientOptions options) : base(options, null)
        {
        }

        private static OkexSocketClientOptions DefaultOptions => defaultOptions.Copy();

        public CallResult<UpdateSubscription> SubscribeToKlineStream(string[] symbols,
                                                                     KlineInterval interval,
                                                                     Action<OkexStreamKlineData> onMessage)
        {
            throw new NotImplementedException();
        }

        public CallResult<UpdateSubscription> SubscribeToKlineStream(string[] symbols,
                                                                     KlineInterval[] intervals,
                                                                     Action<OkexStreamKlineData> onMessage)
        {
            throw new NotImplementedException();
        }

        Task<CallResult<UpdateSubscription>> IOkexWebSocketClient.SubscribeToKlineStreamAsync(string[] symbols,
                                                                                              KlineInterval interval,
                                                                                              Action<OkexStreamKlineData> onMessage)
        {
            return SubscribeToKlineStreamAsync(symbols, interval, onMessage);
        }

        public CallResult<UpdateSubscription> SubscribeToKlineStreamAsync(string[] symbols,
                                                                          KlineInterval[] intervals,
                                                                          Action<OkexStreamKlineData> onMessage)
        {
            throw new NotImplementedException();
        }


        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineStreamAsync(string[] symbols,
                                                                                      KlineInterval interval,
                                                                                      Action<OkexStreamKlineData> onMessage)
        {
            var handler = new Action<OkexStreamKlineData>(onMessage);
            //symbols = symbols.Select(a => a.ToLower() + KlineStreamEndpoint + "_" + JsonConvert.SerializeObject(interval, new KlineIntervalConverter(false))).ToArray();
            return await Subscribe(string.Join("/", symbols), true, handler).ConfigureAwait(false);
        }

        private void Configure(OkexSocketClientOptions options)
        {
            this.baseCombinedAddress = options.BaseSocketCombinedAddress;
        }

        private async Task<CallResult<UpdateSubscription>> Subscribe<T>(string url,
                                                                        bool combined,
                                                                        Action<T> onData)
        {
            if (combined)
            {
                url = this.baseCombinedAddress + "stream?streams=" + url;
            }
            else
            {
                url = BaseAddress + url;
            }

            var connectResult = await CreateAndConnectSocket(url, onData).ConfigureAwait(false);
            return !connectResult.Success ? new CallResult<UpdateSubscription>(null, connectResult.Error) : new CallResult<UpdateSubscription>(new UpdateSubscription(connectResult.Data), null);
        }

        private async Task<CallResult<SocketSubscription>> CreateAndConnectSocket<T>(string url,
                                                                                     Action<T> onMessage)
        {
            var socket = CreateSocket(url);
            var subscription = new SocketSubscription(socket);
            subscription.MessageHandlers.Add(DataHandlerName, (subs,
                                                               data) => DataHandler(data, onMessage));

            var connectResult = await ConnectSocket(subscription).ConfigureAwait(false);
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

            var desResult = Deserialize<T>(data, false);
            if (!desResult.Success)
            {
                this.log.Write(LogVerbosity.Info, $"Couldn't deserialize data received from stream of type {typeof(T)}: " + desResult.Error);
                return false;
            }

            handler(desResult.Data);
            return true;
        }

        protected override bool SocketReconnect(SocketSubscription subscription,
                                                TimeSpan disconnectedTime)
        {
            throw new NotImplementedException();
        }
    }
}