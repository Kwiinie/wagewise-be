using Azure;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;

namespace WageWise.Infrastructure.Utils
{
    public static class PdfParser
    {
        public static string ExtractText(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();

            try
            {
                using var stream = file.OpenReadStream();
                using var pdf = PdfDocument.Open(stream);

                foreach (Page page in pdf.GetPages())
                {
                    var text = page.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        builder.AppendLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PDF extraction failed: " + ex.Message);
                return string.Empty;
            }

            return builder.ToString();
        }
    }
}
