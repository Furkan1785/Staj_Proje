using SakaryaERP.Models;

namespace SakaryaERP.Data.Repositories;

public interface ICariRepository : IRepository<Cari>
{
    Task<bool> KoduKullanimdaMiAsync(string cariKodu, int? haricTutulacakId = null);

    // Global soft-delete filtresini yok sayar; pasif kayıtlar da dahil olur
    IQueryable<Cari> QueryTumu();
    Task<Cari?> GetByIdTumuAsync(int id);
}
