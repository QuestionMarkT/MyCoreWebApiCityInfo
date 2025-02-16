﻿using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.StaticFiles;
using MyCoreWebApiCityInfo.Models;
using System.IO;
using static System.IO.File;

namespace MyCoreWebApiCityInfo.Controllers;

[Route("api/[controller]"), ApiController]
public class CitiesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<CityDto>> GetCities()
    {
        List<CityDto> cities = CitiesDataStore.Current.Cities;

        return cities.Count is 0 ? NoContent() : Ok(cities);
    }

    [HttpGet("{id}")]
    public ActionResult<CityDto> GetCity(int id)
    {
        CityDto? result = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == id);

        return result is null ? NotFound("404 city not found") : Ok(result);
    }
}

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

[Route("api/cities/{cityId}/[controller]"), ApiController]
public class PointsOfInterestController : ControllerBase
{
    const string poiRoute = nameof(GetPointOfInterest);

    [HttpGet]
    public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
        return city is null ? NotFound("404 city not found") : Ok(city.PointsOfInterest);
    }

    [HttpGet($"{{{nameof(pointOfInterestId)}}}", Name = poiRoute)]
    public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        if(ModelState.IsValid is false)
            return BadRequest();

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
                pointOfInterestId = newPoint.Id
            },
            newPoint);
    }

    [HttpPut($"{{{nameof(poiIdFromUser)}}}")]
    public ActionResult UpdatePointOfInterest(int cityId, int poiIdFromUser, PointOfInterestForUpdateDto poiFromUser)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
        if(city is null)
            return NotFound("404 city not found");

        PointOfInterestDto? poiFromMemory = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiIdFromUser);
        if(poiFromMemory is null)
            return NotFound("404 point of interest not found");

        poiFromMemory.Name = poiFromUser.Name;
        poiFromMemory.Description = poiFromUser.Description;

        return NoContent();
    }

    [HttpPatch($"{{{nameof(poiId)}}}")]
    public ActionResult PartiallyUpdatePointOfInterest(int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
        if(city is null)
            return NotFound($"city ID {cityId} not found");

        PointOfInterestDto? poiFromMemory = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
        if(poiFromMemory is null)
            return NotFound($"point of interest of ID {poiId} not found");

        PointOfInterestForUpdateDto poiToPatch = new()
        {
            Name = poiFromMemory.Name,
            Description = poiFromMemory.Description
        };

        patchDocument.ApplyTo(poiToPatch, ModelState);

        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if(!TryValidateModel(poiToPatch))
        {
            return BadRequest(ModelState);
        }

        poiFromMemory.Name = poiToPatch.Name;
        poiFromMemory.Description = poiToPatch.Description;

        return NoContent();
    }

    [HttpDelete($"{{{nameof(poiId)}}}")]
    public ActionResult DeletePointOfInterest(int cityId, int poiId)
    {
        CityDto? cityDto = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        if(cityDto is null)
            return NotFound();

        PointOfInterestDto? poiDtoFromMemory = cityDto.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
        if(poiDtoFromMemory is null)
            return NotFound();

        cityDto.PointsOfInterest.Remove(poiDtoFromMemory);
        return NoContent();
    }
}