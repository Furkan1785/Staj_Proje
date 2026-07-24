using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IAlisIrsaliyesiService
{
    Task<AlisIrsaliyesi> CreateAsync(AlisIrsaliyesi irsaliye, List<AlisIrsaliyesiKalemi> kalemler);

    Task<(IEnumerable<AlisIrsaliyesi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<AlisIrsaliyesi?> GetByIdDetayAsync(int id);

    // Onaylandığında ilgili malzemelerin stok bakiyesini artırır (transaction: tek SaveChangesAsync).
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);
}
