namespace WebApi.WebSocketClients
{
    using System;
    using System.Threading.Tasks;
    using CryptoExchange.Net.Interfaces;
    using CryptoExchange.Net.Objects;
    using CryptoExchange.Net.Sockets;
    using Objects;
    using Objects.Okex;

    /// <summary>
    ///     IOkexWebSocketClient interface
    /// </summary>
    public interface IOkexWebSocketClient
    {
        /// <summary>
        ///     The factory for creating sockets. Used for unit testing
        /// </summary>
        IWebsocketFactory SocketFactory { get; set; }

        /// <summary>
        ///     Subscribes to the candlestick update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="interval">The interval of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>
        ///     A stream subscription. This stream subscription can be used to be notified when the socket is
        ///     disconnected/reconnected
        /// </returns>
        CallResult<UpdateSubscription> SubscribeToKlineStream(string[] symbols,
                                                              KlineInterval interval,
                                                              Action<OkexStreamKlineData> onMessage);

        /// <summary>
        ///     Subscribes to the candlestick update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="intervals">The interval of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>
        ///     A stream subscription. This stream subscription can be used to be notified when the socket is
        ///     disconnected/reconnected
        /// </returns>
        CallResult<UpdateSubscription> SubscribeToKlineStream(string[] symbols,
                                                              KlineInterval[] intervals,
                                                              Action<OkexStreamKlineData> onMessage);

        /// <summary>
        ///     Subscribes to the candlestick update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="interval">The interval of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>
        ///     A stream subscription. This stream subscription can be used to be notified when the socket is
        ///     disconnected/reconnected
        /// </returns>
        Task<CallResult<UpdateSubscription>> SubscribeToKlineStreamAsync(string[] symbols,
                                                                         KlineInterval interval,
                                                                         Action<OkexStreamKlineData> onMessage);

        /// <summary>
        ///     Subscribes to the candlestick update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="intervals">The interval of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>
        ///     A stream subscription. This stream subscription can be used to be notified when the socket is
        ///     disconnected/reconnected
        /// </returns>
        CallResult<UpdateSubscription> SubscribeToKlineStreamAsync(string[] symbols,
                                                                   KlineInterval[] intervals,
                                                                   Action<OkexStreamKlineData> onMessage);
    }
}