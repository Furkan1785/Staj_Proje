using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ICariService
{
    Task<IEnumerable<Cari>> GetAllAsync();
    Task<Cari?> GetByIdAsync(int id);
    Task<Cari> CreateAsync(Cari cari);
    Task UpdateAsync(Cari cari);
    Task PasifYapAsync(int id);
    Task AktifEtAsync(int id);

    Task<(IEnumerable<Cari> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);
}
