using Microsoft.EntityFrameworkCore.Storage;
using SakaryaERP.Data.Repositories;
using SakaryaERP.Models;

namespace SakaryaERP.Data;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();

    // Birden fazla SaveChangesAsync çağrısının (örn. başka bir servisin CreateAsync'i +
    // buradaki durum güncellemesi) tek bir "ya hep ya hiç" birim olarak commit/rollback
    // edilmesi gerektiğinde kullanılır.
    Task<IDbContextTransaction> BeginTransactionAsync();
}