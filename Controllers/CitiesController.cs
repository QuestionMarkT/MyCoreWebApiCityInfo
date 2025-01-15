using MyCoreWebApiCityInfo.Models;
namespace MyCoreWebApiCityInfo.Controllers;

[ApiController, Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<CityDto>> GetCities(byte method)
    {
        List<CityDto> cities = CitiesDataStore.Current.Cities;

        return cities.Count is 0 ? NoContent() : Ok(cities);
    }

    [HttpGet("{id}")]
    public ActionResult<CityDto> GetCity(int id)
    {
        CityDto? result = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == id);
        
        return result is null ? NotFound("City not found") : Ok(result);
    }
}