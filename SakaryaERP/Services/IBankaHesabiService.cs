using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IBankaHesabiService
{
    Task<IEnumerable<BankaHesabi>> GetAllAsync();
    Task<BankaHesabi?> GetByIdAsync(int id);
    Task<BankaHesabi> CreateAsync(BankaHesabi bankaHesabi);
    Task UpdateAsync(BankaHesabi bankaHesabi);
    Task PasifYapAsync(int id);
    Task AktifEtAsync(int id);
}
