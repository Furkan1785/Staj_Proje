using SakaryaERP.Data.Repositories;
using SakaryaERP.Models;

namespace SakaryaERP.Data;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
}