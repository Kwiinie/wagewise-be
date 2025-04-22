using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Application.Interfaces.Services;
using WageWise.Domain.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Google.Apis.Drive.v3;

namespace WageWise.Infrastructure.Services
{
    public class GoogleSheetService : IGoogleSheetService
    {
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;

        public GoogleSheetService(IHostEnvironment env, IConfiguration config)
        {
            var base64Json = config["GoogleServiceAccount:Json"];
            if (string.IsNullOrWhiteSpace(base64Json))
                throw new Exception("Missing GoogleServiceAccount:Json in configuration.");

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64Json));
            var credential = GoogleCredential.FromJson(json)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WageWise"
            });

            _spreadsheetId = config["GoogleSheets:SpreadsheetId"] ?? throw new Exception("Missing GoogleSheets:SpreadsheetId in configuration.");
        }

        public async Task AppendCVMetaDataAsync(CVMetaData data)
        {
            var readRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, "CVLog!A2:A");
            var readResponse = await readRequest.ExecuteAsync();
            int currentRow = (readResponse.Values?.Count ?? 0) + 2; 

            var range = $"CVLog!A{currentRow}";

            var valueRange = new ValueRange();

            valueRange.Values = new List<IList<object>>
        {
            new List<object>
            {
                currentRow - 1, 
                data.Id,
                data.FileUrl,
                data.FileName,
                data.Province,
                data.District ?? "",
                data.PositionLevel,
                data.JobCategory,
                data.Field,
                data.Location,
                data.EstimatedSalary,
                data.SalaryReason,
                data.UploadedAt.ToString("dd-MM-yyyy HH:mm:ss")
            }
        };

            var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            await appendRequest.ExecuteAsync();
        }
    }
}
