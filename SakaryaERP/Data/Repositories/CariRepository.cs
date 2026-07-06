using Microsoft.EntityFrameworkCore;
using SakaryaERP.Models;

namespace SakaryaERP.Data.Repositories;

public class CariRepository : BaseRepository<Cari>, ICariRepository
{
    public CariRepository(AppDbContext context) : base(context) { }

    public async Task<bool> KoduKullanimdaMiAsync(string cariKodu, int? haricTutulacakId = null)
        => await _dbSet.AnyAsync(c => c.CariKodu == cariKodu
            && (!haricTutulacakId.HasValue || c.Id != haricTutulacakId.Value));
}
