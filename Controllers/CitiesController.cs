namespace MyCoreWebApiCityInfo.Controllers;

[ApiController]
public class CitiesController : ControllerBase
{
    [HttpGet("api/cities")]
    public JsonResult GetCities() => new(
        new List<object>
        {
            new { Id = 1, Name = "New York City" },
            new { Id = 2, Name = "Antwerp" }
        });
}