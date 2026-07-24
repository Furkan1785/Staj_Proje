using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IAlisFaturasiService
{
    Task<AlisFaturasi> CreateAsync(AlisFaturasi fatura, List<AlisFaturasiKalemi> kalemler);

    Task<(IEnumerable<AlisFaturasi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<AlisFaturasi?> GetByIdDetayAsync(int id);
    Task<bool> AktifFaturaVarMiAsync(int alisIrsaliyesiId);

    // Satınalma özet raporu için: onaylanmış tüm faturalar (Cari + Kalemler ile).
    Task<List<AlisFaturasi>> GetOnaylanmisListeAsync();

    // Onaylandığında cariye alacak hareketi (CariFisi) eklenir ve Cari.Bakiye güncellenir (tek transaction).
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);
}
