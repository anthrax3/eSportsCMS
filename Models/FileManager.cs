using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebDevEsports.Models
{
    public class FileManager
    {

        public static async Task<string> UploadFile(IHostingEnvironment _environment, IFormFile file)
        {
            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
            string fileName = Path.GetFileName(file.FileName);

            using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}
