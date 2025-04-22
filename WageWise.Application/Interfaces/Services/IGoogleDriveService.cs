using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WageWise.Application.Interfaces.Services
{
    public interface IGoogleDriveService
    {
        Task<string> EnsureFolderExistsAsync(string name, string? parentId = null);
        Task<(string FileId, string WebViewLink)> UploadFileAsync(IFormFile file, string parentId);
        Task SetEditPermissionForEmailsAsync(string fileId, List<string> emails);
        Task<string> GetRootFolderIdAsync();
    }
}
