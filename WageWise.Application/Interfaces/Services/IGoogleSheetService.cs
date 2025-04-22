using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Domain.Entities;

namespace WageWise.Application.Interfaces.Services
{
    public interface IGoogleSheetService
    {
        Task AppendCVMetaDataAsync(CVMetaData data);
    }
}
