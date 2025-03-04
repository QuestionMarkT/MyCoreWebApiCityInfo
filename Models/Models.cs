using MyCoreWebApiCityInfo.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyCoreWebApiCityInfo.Models;

public class CityWithoutPoi
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public static explicit operator CityWithoutPoi(CityDbEntity cityDto) => new()
    {
        Id = cityDto.Id,
        Name = cityDto.Name,
        Description = cityDto.Description
    };
}
public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int NumberOfPointsOfInterest { get => PointsOfInterest.Count; }
    public ICollection<PointOfInterest> PointsOfInterest { get; set; } = [];

    public static implicit operator City(CityDbEntity sourceCity)
    {
        City castedCity = new()
        {
            Id = sourceCity.Id,
            Name = sourceCity.Name,
            Description = sourceCity.Description,
            PointsOfInterest = sourceCity.PointsOfInterest
                .Select(x => (PointOfInterest) x)
                .ToArray()
        };

        return castedCity;
    }
    
}
public class PointOfInterestForUpdateDto
{
    [Required(ErrorMessage = "Name is required"), MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }    
}
public class PointOfInterestForCreatonDto
{
    [Required(ErrorMessage = "Name is required"), MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }

    public static implicit operator PointOfInterestDBEntity(PointOfInterestForCreatonDto poiForCreation) => new(poiForCreation.Name)
    {
        Description = poiForCreation.Description
    };
}
public class PointOfInterest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public static implicit operator PointOfInterest(PointOfInterestDBEntity dbEnt) => new()
    {
        Id = dbEnt.Id,
        Name = dbEnt.Name,
        Description = dbEnt.Description
    };

    public static implicit operator PointOfInterest(PointOfInterestForCreatonDto poiForCreation) => new()
    {
        Name = poiForCreation.Name,
        Description = poiForCreation.Description
    };
}