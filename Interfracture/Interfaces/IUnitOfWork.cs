using Interfracture.Entities;

namespace Interfracture.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Order> Orders { get; }
        IRepository<ApplicationUser> Users { get; }
        IRepository<Category> Categories { get; }
        IRepository<OrderDetail> OrderDetails { get; }
        IRepository<Drink> Drinks { get; }
        void Save();
    }
}
