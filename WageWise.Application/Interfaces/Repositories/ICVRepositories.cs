using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Domain.Entities;

namespace WageWise.Application.Interfaces.Repositories
{
    public interface ICVRepositories
    {
        public Task SaveAsync(CVMetaData cv);
    }
}
