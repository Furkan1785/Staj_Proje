using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ISubeService
{
    Task<IEnumerable<Sube>> GetAllAsync();
    Task<Sube?> GetByIdAsync(int id);
    Task<Sube> CreateAsync(Sube sube);
    Task UpdateAsync(Sube sube);
    Task PasifYapAsync(int id);
    Task AktifEtAsync(int id);
}
