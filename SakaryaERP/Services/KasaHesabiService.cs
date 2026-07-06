using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class KasaHesabiService : IKasaHesabiService
{
    private readonly IUnitOfWork _unitOfWork;

    public KasaHesabiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<KasaHesabi>> GetAllAsync()
        => await _unitOfWork.Repository<KasaHesabi>().QueryTumu().ToListAsync();

    public async Task<KasaHesabi?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<KasaHesabi>().GetByIdAsync(id);

    public async Task<KasaHesabi> CreateAsync(KasaHesabi kasaHesabi)
    {
        await _unitOfWork.Repository<KasaHesabi>().AddAsync(kasaHesabi);
        await _unitOfWork.SaveChangesAsync();
        return kasaHesabi;
    }

    public async Task UpdateAsync(KasaHesabi kasaHesabi)
    {
        var mevcut = await _unitOfWork.Repository<KasaHesabi>().GetByIdAsync(kasaHesabi.Id)
            ?? throw new InvalidOperationException("Kasa hesabı bulunamadı.");

        mevcut.KasaAdi = kasaHesabi.KasaAdi;
        mevcut.ParaBirimi = kasaHesabi.ParaBirimi;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PasifYapAsync(int id)
    {
        var kasaHesabi = await _unitOfWork.Repository<KasaHesabi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Kasa hesabı bulunamadı.");

        _unitOfWork.Repository<KasaHesabi>().SoftDelete(kasaHesabi);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AktifEtAsync(int id)
    {
        var kasaHesabi = await _unitOfWork.Repository<KasaHesabi>().GetByIdTumuAsync(id)
            ?? throw new InvalidOperationException("Kasa hesabı bulunamadı.");

        kasaHesabi.IsDeleted = false;
        await _unitOfWork.SaveChangesAsync();
    }
}
