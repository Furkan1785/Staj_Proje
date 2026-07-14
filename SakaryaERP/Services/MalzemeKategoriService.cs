using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class MalzemeKategoriService : IMalzemeKategoriService
{
    private readonly IUnitOfWork _unitOfWork;

    public MalzemeKategoriService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MalzemeKategori>> GetAllAsync()
        => await _unitOfWork.Repository<MalzemeKategori>().GetAllAsync();

    public async Task<MalzemeKategori> CreateAsync(MalzemeKategori kategori)
    {
        await _unitOfWork.Repository<MalzemeKategori>().AddAsync(kategori);
        await _unitOfWork.SaveChangesAsync();
        return kategori;
    }
}
