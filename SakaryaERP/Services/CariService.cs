using SakaryaERP.Data;
using SakaryaERP.Data.Repositories;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class CariService : ICariService
{
    private readonly ICariRepository _cariRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CariService(ICariRepository cariRepository, IUnitOfWork unitOfWork)
    {
        _cariRepository = cariRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Cari>> GetAllAsync()
        => await _cariRepository.GetAllAsync();

    public async Task<Cari?> GetByIdAsync(int id)
        => await _cariRepository.GetByIdAsync(id);

    public async Task<Cari> CreateAsync(Cari cari)
    {
        if (await _cariRepository.KoduKullanimdaMiAsync(cari.CariKodu))
            throw new InvalidOperationException("Bu cari kodu zaten kullanılıyor.");

        await _cariRepository.AddAsync(cari);
        await _unitOfWork.SaveChangesAsync();
        return cari;
    }

    public async Task UpdateAsync(Cari cari)
    {
        if (await _cariRepository.KoduKullanimdaMiAsync(cari.CariKodu, cari.Id))
            throw new InvalidOperationException("Bu cari kodu zaten kullanılıyor.");

        _cariRepository.Update(cari);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PasifYapAsync(int id)
    {
        var cari = await _cariRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        _cariRepository.SoftDelete(cari);
        await _unitOfWork.SaveChangesAsync();
    }
}
