namespace WebApi.Services
{
    using System.Threading.Tasks;
    using Core.Data.Collections;

    public interface IHuobiService
    {
        Task AddKlineData(HuobiKlineDocument data);

        Task AddKlineArrayData(HuobiKlineDocument[] array);
    }
}