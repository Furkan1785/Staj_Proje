using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Services;

public class MizanService : IMizanService
{
    private readonly IUnitOfWork _unitOfWork;

    public MizanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MizanViewModel> GetMizanAsync(DateTime? baslangic, DateTime? bitis)
    {
        var araligBaslangic = baslangic ?? new DateTime(DateTime.Today.Year, 1, 1);
        var araligBitis = bitis ?? DateTime.Today;
        if (araligBaslangic > araligBitis)
            throw new InvalidOperationException("Başlangıç tarihi bitiş tarihinden sonra olamaz.");

        // QueryTumu() global soft-delete filtresini atlar (IgnoreQueryFilters) — SatisFaturasi/
        // AlisFaturasi iptal edildiğinde MuhasebeFisi soft-delete edilir (bkz. IptalEtAsync);
        // bunlar mizana dahil edilmemeli, aksi halde iptal edilmiş bir faturanın yevmiye kaydı
        // hâlâ Borç/Alacak toplamlarını şişirir (her iki taraf da eşit şiştiği için "Borç=Alacak"
        // denge kontrolü bu hatayı gizler).
        var satirlar = await _unitOfWork.Repository<MuhasebeFisiKalemi>().QueryTumu()
            .Where(k => !k.MuhasebeFisi.IsDeleted
                && k.MuhasebeFisi.Tarih >= araligBaslangic && k.MuhasebeFisi.Tarih <= araligBitis)
            .GroupBy(k => new { k.HesapPlani.HesapKodu, k.HesapPlani.HesapAdi })
            .Select(g => new MizanSatiriViewModel
            {
                HesapKodu = g.Key.HesapKodu,
                HesapAdi = g.Key.HesapAdi,
                ToplamBorc = g.Sum(k => k.Borc),
                ToplamAlacak = g.Sum(k => k.Alacak)
            })
            .OrderBy(s => s.HesapKodu)
            .ToListAsync();

        return new MizanViewModel { Baslangic = araligBaslangic, Bitis = araligBitis, Satirlar = satirlar };
    }
}
