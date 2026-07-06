using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IKasaHesabiService
{
    Task<IEnumerable<KasaHesabi>> GetAllAsync();
    Task<KasaHesabi?> GetByIdAsync(int id);
    Task<KasaHesabi> CreateAsync(KasaHesabi kasaHesabi);
    Task UpdateAsync(KasaHesabi kasaHesabi);
    Task PasifYapAsync(int id);
    Task AktifEtAsync(int id);
}
