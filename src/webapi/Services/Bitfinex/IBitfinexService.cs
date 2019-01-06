namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Core.Data.Collections;

    public interface IBitfinexService
    {
        Task AddKlineData(BitfinexKlineDocument data);

        Task AddKlineArrayData(BitfinexKlineDocument[] array);
    }
}