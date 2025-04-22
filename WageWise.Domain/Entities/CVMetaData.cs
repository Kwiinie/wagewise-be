using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WageWise.Domain.Entities
{
    public class CVMetaData
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string Province { get; set; }
        public string? District { get; set; }
        public string PositionLevel { get; set; }
        public string JobCategory { get; set; }
        public string Field {  get; set; }
        public string Location { get; set; }
        public int EstimatedSalary { get; set; }
        public string SalaryReason { get; set; }
        public string ImprovementSuggestions {  get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
