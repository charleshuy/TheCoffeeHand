using Interfracture.Entities;

namespace Interfracture.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        void Dispose();
        void Save();
        Task SaveAsync();
    }
}
