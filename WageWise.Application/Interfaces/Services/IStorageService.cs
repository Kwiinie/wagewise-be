using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WageWise.Application.Interfaces.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
