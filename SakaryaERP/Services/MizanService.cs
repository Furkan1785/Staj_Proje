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

        var satirlar = await _unitOfWork.Repository<MuhasebeFisiKalemi>().QueryTumu()
            .Where(k => k.MuhasebeFisi.Tarih >= araligBaslangic && k.MuhasebeFisi.Tarih <= araligBitis)
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
