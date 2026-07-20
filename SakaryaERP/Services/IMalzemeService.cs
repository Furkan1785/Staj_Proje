using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IMalzemeService
{
    Task<(IEnumerable<Malzeme> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<Malzeme> CreateAsync(Malzeme malzeme);
    Task<Malzeme?> GetByIdAsync(int id);
    Task UpdateAsync(Malzeme malzeme);
    Task<IEnumerable<Malzeme>> GetTumListeAsync();
    Task<MalzemeIceAktarSonucu> TopluIceAktarAsync(List<MalzemeImportSatiri> satirlar);

    // Bakiyesi MinStokMiktari'nın altına düşen malzemeler — Gün 14'teki uyarı paneli
    // ve ileride Dashboard/Gün 15'teki kritik stok raporu için ortak sorgu.
    Task<IEnumerable<Malzeme>> GetKritikStokListesiAsync();
}
