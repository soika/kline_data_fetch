namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Collections;

    public interface ICoinExService
    {
        Task AddKlineData(CoinExKlineDocument data);

        Task AddKlineArrayData(CoinExKlineDocument[] array);
    }
}