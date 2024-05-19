using Bulky.DataAccess.Repository.IRepository;
using BulkyWeb.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category {  get; private set; }
        public IProductRepository Product {  get; private set; }
        public ICompanyRepository Company {  get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; }
        public IOrderHeaderRepository orderHeader { get; private set; }
        public IOrderDetailRepository orderDetail { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }

        ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Product = new ProductRepository(_context);
            Company = new CompanyRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            ApplicationUser=new ApplicationUserRepository(_context);
            orderHeader=new OrderHeaderRepository(_context);
            orderDetail=new OrderDetailRepository(_context);
            ProductImage=new ProductImageRepository(_context);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
