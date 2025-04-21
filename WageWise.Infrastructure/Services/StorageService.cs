using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Application.Interfaces.Services;

namespace WageWise.Infrastructure.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public StorageService(IConfiguration config)
        {
            _connectionString = config["AzureBlob:ConnectionString"] ?? throw new ArgumentNullException("AzureBlob:ConnectionString");
            _containerName = config["AzureBlob:ContainerName"] ?? "cv-files";
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}
