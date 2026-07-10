using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ICekSenetService
{
    Task<IEnumerable<CekSenet>> GetAllAsync();
    Task<CekSenet?> GetByIdAsync(int id);
    Task<CekSenet> CreateAsync(CekSenet cekSenet);
    Task UpdateAsync(CekSenet cekSenet);
    Task<(IEnumerable<CekSenet> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task TahsileVerAsync(int id);
    Task CiroEtAsync(int id, string ciroBilgisi);
    Task TahsilEdildiYapAsync(int id, int? bankaHesabiId, int? kasaHesabiId);
    Task KarsiliksizYapAsync(int id);
}
