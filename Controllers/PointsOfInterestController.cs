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
        return city is null ? NotFound("404 city not found") : Ok(city.PointsOfInterest);
    }

    [HttpGet("{pointOfInterestId}")]
    public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);

        if(city is null)
            return NotFound("404 city not found");

        PointOfInterestDto? poi = city.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
        return poi is null ? NotFound("404 point of interest not found") : Ok(poi);
    }
}