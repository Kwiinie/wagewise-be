using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Domain.Commons;
using WageWise.Domain.Entities;

namespace WageWise.Application.Interfaces.Services
{
    public interface ICVService
    {
        Task<Result<CVMetaData>> HandleAsync(IFormFile file, string province, string district);
    }
}
