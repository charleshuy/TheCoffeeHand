using Interfracture.Entities;
using Interfracture.Interfaces;
using Repositories.Base;

namespace Repositories.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<ApplicationUser> _users;
        private IRepository<Category> _categories;
        private IRepository<Drink> _drinks;
        private IRepository<Order> _orders;
        private IRepository<OrderDetail> _orderDetails;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<ApplicationUser> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new Repository<ApplicationUser>(_context);
                }
                return _users;
            }
        }

        public IRepository<Category> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new Repository<Category>(_context);
                }
                return _categories;
            }
        }

        public IRepository<Drink> Drinks
        {
            get
            {
                if (_drinks == null)
                {
                    _drinks = new Repository<Drink>(_context);
                }
                return _drinks;
            }
        }

        public IRepository<Order> Orders
        {
            get
            {
                if (_orders == null)
                {
                    _orders = new Repository<Order>(_context);
                }
                return _orders;
            }
        }

        public IRepository<OrderDetail> OrderDetails
        {
            get
            {
                if (_orderDetails == null)
                {
                    _orderDetails = new Repository<OrderDetail>(_context);
                }
                return _orderDetails;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
