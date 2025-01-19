using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCoreWebApiCityInfo.Models;

namespace MyCoreWebApiCityInfo.Controllers;

[Route("api/cities/{cityId}/[controller]"), ApiController]
public class PointsOfInterestController : ControllerBase
{
    const string poiRoute = "GetPointOfInterest";

    [HttpGet]
    public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
        return city is null ? NotFound("404 city not found") : Ok(city.PointsOfInterest);
    }

    [HttpGet("{pointOfInterestId}", Name = poiRoute)]
    public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);

        if(city is null)
            return NotFound("404 city not found");

        PointOfInterestDto? poi = city.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
        return poi is null ? NotFound("404 point of interest not found") : Ok(poi);
    }

    [HttpPost]
    public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreatonDto poi)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        if(city is null)
            return NotFound();

        int maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id); // a temporary solution that will stay here for good

        PointOfInterestDto newPoint = new()
        {
            Id = ++maxPointOfInterestId,
            Name = poi.Name,
            Description = poi.Description
        };

        city.PointsOfInterest.Add(newPoint);
        return CreatedAtRoute(poiRoute,
            new
            {
                cityId,
                poiId = newPoint.Id
            },
            newPoint); 
    }
}