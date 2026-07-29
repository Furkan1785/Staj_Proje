using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IHesapPlaniService
{
    Task<IEnumerable<HesapPlani>> GetAllAsync();
    Task<HesapPlani> CreateAsync(HesapPlani hesap);
    Task<HesapPlani> GetByKoduAsync(string hesapKodu);
}
