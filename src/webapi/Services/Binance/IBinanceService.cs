namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Core.Data.Collections;

    public interface IBinanceService
    {
        Task AddKlineData(BinanceKlineDocument data);

        Task AddKlineArrayData(BinanceKlineDocument[] array);
    }
}