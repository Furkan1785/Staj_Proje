using SakaryaERP.Models;

namespace SakaryaERP.Data.Repositories;

public interface ICariRepository : IRepository<Cari>
{
    Task<bool> KoduKullanimdaMiAsync(string cariKodu, int? haricTutulacakId = null);
}
