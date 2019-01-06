namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Core.Data.Collections;

    public interface IBittrexService
    {
        Task AddKlineData(BittrexKlineDocument data);

        Task AddKlineArrayData(BittrexKlineDocument[] array);
    }
}