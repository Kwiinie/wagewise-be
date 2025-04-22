using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Domain.Commons;
using WageWise.Domain.Entities;
using WageWise.Domain.Interfaces;
using WageWise.Infrastructure.Utils;
using WageWise.Application.Interfaces.Services;

namespace WageWise.Infrastructure.Services
{
    public class CVServices : ICVService
    {
        private readonly IStorageService _storage;
        private readonly IAIService _AI;
        private readonly IUnitOfWork _unitOfWork;

        public CVServices(
            IStorageService storage,
            IAIService AI,
            IUnitOfWork unitOfWork)
        {
            _storage = storage;
            _AI = AI;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CVMetaData>> HandleAsync(IFormFile file, string province, string district)
        {
            if (file == null || file.Length == 0)
                return Result<CVMetaData>.Failure("No file uploaded.");

            var fileUrl = await _storage.UploadFileAsync(file);

            var extractedText = PdfParser.ExtractText(file);

            if (string.IsNullOrWhiteSpace(extractedText))
                return Result<CVMetaData>.Failure("Unable to extract text from PDF.");

            var (isCV, reason) = await _AI.IsLikelyCVAsync(extractedText);
            if (!isCV)
                return Result<CVMetaData>.Failure($"Tệp không phải là CV hợp lệ. {reason}");

            var analysis = await _AI.AnalyzeCVAsync(extractedText, province, district);
            if (analysis == null)
                return Result<CVMetaData>.Failure("AI analysis failed.");

            var metadata = new CVMetaData
            {
                Id = Guid.NewGuid(),
                FileUrl = fileUrl,
                FileName = file.FileName,
                Province = province,
                District = district,
                PositionLevel = analysis.PositionLevel,
                Field = analysis.Field,
                JobCategory = analysis.JobCategory,
                SalaryReason = analysis.SalaryReason,
                Location = analysis.Location,
                EstimatedSalary = analysis.EstimatedSalary,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.CVs.AddAsync(metadata);
            await _unitOfWork.SaveChangesAsync();

            return Result<CVMetaData>.Success(metadata);
        }
    }
}
