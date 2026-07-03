using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ICariService
{
    Task<IEnumerable<Cari>> GetAllAsync();
    Task<Cari?> GetByIdAsync(int id);
    Task<Cari> CreateAsync(Cari cari);
    Task UpdateAsync(Cari cari);
    Task PasifYapAsync(int id);
}
