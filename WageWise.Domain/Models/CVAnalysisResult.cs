using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WageWise.Domain.Models
{
    public class CVAnalysisResult
    {
        public string PositionLevel { get; set; } = "";
        public string JobCategory { get; set; } = "";
        public string Field { get; set; } = "";
        public string Location { get; set; } = "";
        public int EstimatedSalary { get; set; }
        public string SalaryReason {  get; set; } 
        public string ImprovementSuggestions { get; set; }
    }
}
