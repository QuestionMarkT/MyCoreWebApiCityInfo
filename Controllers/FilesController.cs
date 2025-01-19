namespace MyCoreWebApiCityInfo.Controllers;

[Route("api/[controller]"), ApiController]
public class FilesController : ControllerBase
{
    [HttpGet("{fileId}")]
    public ActionResult GetFile(string fileId)
    {
        FileContentResult rp;
        FileStreamResult result;
        PhysicalFileResult physicalFileResult;
        VirtualFileResult virtualFileResult = new("","");
        return null;
    }
}
