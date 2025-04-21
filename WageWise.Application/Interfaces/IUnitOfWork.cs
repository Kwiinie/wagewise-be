using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Application.Interfaces.Repositories;
using WageWise.Domain.Entities;

namespace WageWise.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<CVMetaData> CVs { get; }
        Task<int> SaveChangesAsync();
    }

}
