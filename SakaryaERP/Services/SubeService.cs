using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class SubeService : ISubeService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Sube>> GetAllAsync()
        => await _unitOfWork.Repository<Sube>().QueryTumu().ToListAsync();

    public async Task<Sube?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<Sube>().GetByIdAsync(id);

    public async Task<Sube> CreateAsync(Sube sube)
    {
        await _unitOfWork.Repository<Sube>().AddAsync(sube);
        await _unitOfWork.SaveChangesAsync();
        return sube;
    }

    public async Task UpdateAsync(Sube sube)
    {
        var mevcut = await _unitOfWork.Repository<Sube>().GetByIdAsync(sube.Id)
            ?? throw new InvalidOperationException("Şube bulunamadı.");

        mevcut.SubeAdi = sube.SubeAdi;
        mevcut.Adres = sube.Adres;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PasifYapAsync(int id)
    {
        var sube = await _unitOfWork.Repository<Sube>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Şube bulunamadı.");

        _unitOfWork.Repository<Sube>().SoftDelete(sube);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AktifEtAsync(int id)
    {
        var sube = await _unitOfWork.Repository<Sube>().GetByIdTumuAsync(id)
            ?? throw new InvalidOperationException("Şube bulunamadı.");

        sube.IsDeleted = false;
        await _unitOfWork.SaveChangesAsync();
    }
}
