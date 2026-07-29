using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class HesapPlaniService : IHesapPlaniService
{
    private readonly IUnitOfWork _unitOfWork;

    public HesapPlaniService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HesapPlani>> GetAllAsync()
        => await _unitOfWork.Repository<HesapPlani>().QueryTumu()
            .OrderBy(h => h.HesapKodu)
            .ToListAsync();

    public async Task<HesapPlani> CreateAsync(HesapPlani hesap)
    {
        await _unitOfWork.Repository<HesapPlani>().AddAsync(hesap);
        await _unitOfWork.SaveChangesAsync();
        return hesap;
    }

    public async Task<HesapPlani> GetByKoduAsync(string hesapKodu)
        => await _unitOfWork.Repository<HesapPlani>().QueryTumu()
            .FirstOrDefaultAsync(h => h.HesapKodu == hesapKodu)
            ?? throw new InvalidOperationException($"{hesapKodu} kodlu hesap, hesap planında bulunamadı.");
}
