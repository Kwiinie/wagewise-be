using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using WageWise.Application.Interfaces.Services;




namespace WageWise.Infrastructure.Services
{

    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _rootFolderId;
        private readonly List<string> _editorEmails;

        public GoogleDriveService(IHostEnvironment env, IConfiguration config)
        {
            var credential = GoogleCredential.FromFile(Path.Combine(env.ContentRootPath, "google-service-account.json"))
                .CreateScoped(DriveService.Scope.DriveFile);

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WageWise"
            });

            _rootFolderId = config["GoogleDrive:RootFolderId"] ?? throw new Exception("Missing GoogleDrive:RootFolderId in configuration.");
            _editorEmails = config.GetSection("GoogleDrive:EditorEmails").Get<List<string>>() ?? new List<string>();
        }

        public async Task<string> EnsureFolderExistsAsync(string name, string? parentId = null)
        {
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{name}'"
                          + (parentId != null ? $" and '{parentId}' in parents" : "");
            listRequest.Fields = "files(id, name)";
            var list = await listRequest.ExecuteAsync();

            if (list.Files.Any())
                return list.Files.First().Id;

            var folderMetadata = new File
            {
                Name = name,
                Description = "Thư mục chứa CV theo tháng và vị trí",
                MimeType = "application/vnd.google-apps.folder",
                Parents = parentId != null ? new List<string> { parentId } : null
            };

            var createRequest = _driveService.Files.Create(folderMetadata);
            createRequest.Fields = "id";
            var folder = await createRequest.ExecuteAsync();
            return folder.Id;
        }

        public async Task<(string FileId, string WebViewLink)> UploadFileAsync(IFormFile file, string parentId)
        {
            var fileMetadata = new File
            {
                Name = file.FileName,
                Description = "CV ứng viên tự động tải lên từ hệ thống WageWise",
                Parents = new List<string> { parentId }
            };

            using var stream = file.OpenReadStream();
            var uploadRequest = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
            uploadRequest.Fields = "id, webViewLink";

            await uploadRequest.UploadAsync();
            var uploadedFile = uploadRequest.ResponseBody;

            await SetEditPermissionForEmailsAsync(uploadedFile.Id, _editorEmails);

            if (!string.IsNullOrWhiteSpace(uploadedFile.WebViewLink))
            {
                using var httpClient = new HttpClient();
                try
                {
                    await httpClient.GetAsync(uploadedFile.WebViewLink);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DriveService] Warning: Cannot access uploaded file. {ex.Message}");
                }
            }

            return (uploadedFile.Id, uploadedFile.WebViewLink);
        }

        public async Task SetEditPermissionForEmailsAsync(string fileId, List<string> emails)
        {
            foreach (var email in emails)
            {
                var permission = new Permission
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = email
                };

                var request = _driveService.Permissions.Create(permission, fileId);
                request.Fields = "id";
                await request.ExecuteAsync();
            }
        }

        public Task<string> GetRootFolderIdAsync()
        {
            return Task.FromResult(_rootFolderId);
        }
    }

}
