using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Application.Interfaces.Repositories;
using WageWise.Domain.Entities;
using WageWise.Domain.Interfaces;
using WageWise.Infrastructure.Data;

namespace WageWise.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            CVs = new GenericRepository<CVMetaData>(_db);
        }

        public IGenericRepository<CVMetaData> CVs { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }

}
