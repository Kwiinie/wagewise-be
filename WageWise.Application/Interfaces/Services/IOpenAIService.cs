using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Domain.Models;

namespace WageWise.Application.Interfaces.Services
{
    public interface IAIService
    {
        Task<CVAnalysisResult?> AnalyzeCVAsync(string textContent, string province, string district);
        Task<(bool isCV, string reason)> IsLikelyCVAsync(string textContent);

    }
}
