using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using static System.IO.File;

namespace MyCoreWebApiCityInfo.Controllers;

[Route("api/[controller]"), ApiController]
public class FilesController(FileExtensionContentTypeProvider fectp) : ControllerBase
{
    readonly FileExtensionContentTypeProvider _fectp = fectp ?? throw new ArgumentNullException(nameof(fectp) + " is null in FilesController class");

    [HttpGet("{fileId}")]
    public ActionResult GetFile(string fileId)
    {
#if true
        string filePath = "blank PDF.pdf";
#else
        string filePath = "blank document no pw.pdf";
#endif
        if(!Exists(filePath))
            return NotFound();

        if(!_fectp.TryGetContentType(filePath, out string? contentType))
            contentType = "application/octet-stream";

        byte[] fileBytes = ReadAllBytes(filePath);
        return File(fileBytes, contentType, Path.GetFileName(filePath));
    }
}