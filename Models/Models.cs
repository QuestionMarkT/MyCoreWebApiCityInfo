using MyCoreWebApiCityInfo.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyCoreWebApiCityInfo.Models;

public class CityWithoutPoiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public static explicit operator CityWithoutPoiDto(City cityDto) => new()
    {
        Id = cityDto.Id,
        Name = cityDto.Name,
        Description = cityDto.Description
    };
}

public class CityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int NumberOfPointsOfInterest { get => PointsOfInterest.Count; }
    public ICollection<PointOfInterestDto> PointsOfInterest { get; set; } = [];
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
    [Required(ErrorMessage = "Name is required"), MaxLength(50)] // https://docs.fluentvalidation.net/en/latest/ for more advanced input validation
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }
}
public class PointOfInterestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}