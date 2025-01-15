using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCoreWebApiCityInfo.Models;

namespace MyCoreWebApiCityInfo.Controllers;

[Route("api/cities/{cityId}/[controller]"), ApiController]
public class PointsOfInterestController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
        return city is null ? NotFound("404 city ID not found") : Ok(city.PointsOfInterest);
    }
}