using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SakaryaERP.Data.Repositories;
using SakaryaERP.Models;

namespace SakaryaERP.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new BaseRepository<T>(_context);
            _repositories[type] = repo;
        }
        return (IRepository<T>)repo;
    }

    // DbUpdateConcurrencyException (xmin uyuşmazlığı — bkz. AppDbContext.OnModelCreating) tüm
    // servislerde tek noktadan yakalanıp kullanıcıya anlaşılır bir mesajla iletilsin diye burada
    // ele alınıyor; her Onayla/IptalEt metoduna ayrı ayrı try/catch eklemeye gerek kalmıyor.
    // Mevcut controller'lar zaten InvalidOperationException'ı yakalayıp TempData'ya yazıyor.
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "Bu kayıt sizden hemen önce başka bir kullanıcı tarafından değiştirildi. " +
                "Sayfayı yenileyip tekrar deneyin.");
        }
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
        => _context.Database.BeginTransactionAsync();

    public void Dispose()
        => _context.Dispose();
}