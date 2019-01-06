namespace Core.Shared.Mappers
{
    using AutoMapper;
    using Binance.Net.Objects;
    using Bitfinex.Net.Objects;
    using CoinEx.Net.Objects;
    using Data.Collections;
    using Huobi.Net.Objects;
    using WebApi.Collections;

    public class ClassesMapperProfile : Profile
    {
        public ClassesMapperProfile()
        {
            CreateMap<CoinExKline, CoinExKlineDocument>()
               .ForMember(x => x.Symbol, opt => opt.Ignore())
               .AfterMap((kline,
                          document) =>
                {
                    document.Symbol = kline.Market;
                });

            CreateMap<HuobiMarketKline, HuobiKlineDocument>()
               .ForMember(x => x.Id, opt => opt.Ignore())
               .ForMember(x => x.Symbol, opt => opt.Ignore())
               .AfterMap((kline,
                          document) =>
                {
                    document.Timestamp = kline.Id;
                });

            CreateMap<BitfinexCandle, BitfinexKlineDocument>()
               .ForMember(x => x.Id, opt => opt.Ignore())
               .ForMember(x => x.Symbol, opt => opt.Ignore());

            CreateMap<BinanceKline, BinanceKlineDocument>()
               .ForMember(x => x.Id, opt => opt.Ignore())
               .ForMember(x => x.Symbol, opt => opt.Ignore());
        }
    }
}