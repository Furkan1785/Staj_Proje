using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ISatisTeklifiService
{
    Task<SatisTeklifi> CreateAsync(SatisTeklifi teklif, List<SatisTeklifiKalemi> kalemler);

    Task<(IEnumerable<SatisTeklifi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<SatisTeklifi?> GetByIdDetayAsync(int id);
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);

    // Belge takip ekranı: geçerlilik tarihi geçen/yaklaşan tekliflerin tespiti için.
    Task<List<SatisTeklifi>> GetBeklemedeListesiAsync();
}
