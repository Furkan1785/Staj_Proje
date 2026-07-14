using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IMalzemeKategoriService
{
    Task<IEnumerable<MalzemeKategori>> GetAllAsync();
    Task<MalzemeKategori> CreateAsync(MalzemeKategori kategori);
}
