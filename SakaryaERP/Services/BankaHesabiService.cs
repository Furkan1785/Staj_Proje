using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class BankaHesabiService : IBankaHesabiService
{
    private readonly IUnitOfWork _unitOfWork;

    public BankaHesabiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<BankaHesabi>> GetAllAsync()
        => await _unitOfWork.Repository<BankaHesabi>().QueryTumu().ToListAsync();

    public async Task<BankaHesabi?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<BankaHesabi>().GetByIdAsync(id);

    public async Task<BankaHesabi> CreateAsync(BankaHesabi bankaHesabi)
    {
        await _unitOfWork.Repository<BankaHesabi>().AddAsync(bankaHesabi);
        await _unitOfWork.SaveChangesAsync();
        return bankaHesabi;
    }

    public async Task UpdateAsync(BankaHesabi bankaHesabi)
    {
        var mevcut = await _unitOfWork.Repository<BankaHesabi>().GetByIdAsync(bankaHesabi.Id)
            ?? throw new InvalidOperationException("Banka hesabı bulunamadı.");

        // Bakiye alanı forma gelmiyor (CariFisi hareketleriyle otomatik güncellenecek), o yüzden ellenmiyor
        mevcut.HesapAdi = bankaHesabi.HesapAdi;
        mevcut.BankaAdi = bankaHesabi.BankaAdi;
        mevcut.IBAN = bankaHesabi.IBAN;
        mevcut.ParaBirimi = bankaHesabi.ParaBirimi;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PasifYapAsync(int id)
    {
        var bankaHesabi = await _unitOfWork.Repository<BankaHesabi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Banka hesabı bulunamadı.");

        _unitOfWork.Repository<BankaHesabi>().SoftDelete(bankaHesabi);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AktifEtAsync(int id)
    {
        var bankaHesabi = await _unitOfWork.Repository<BankaHesabi>().GetByIdTumuAsync(id)
            ?? throw new InvalidOperationException("Banka hesabı bulunamadı.");

        bankaHesabi.IsDeleted = false;
        await _unitOfWork.SaveChangesAsync();
    }
}
