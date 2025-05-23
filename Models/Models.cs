﻿#pragma warning disable CS1591
using MyCoreWebApiCityInfo.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyCoreWebApiCityInfo.Models;

/// <summary>
/// A city without points of interest
/// </summary>
public class CityWithoutPoi
{
    /// <summary>
    /// ID of the city
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the city
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// The description of the city
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Map to city without points of interest
    /// </summary>
    /// <param name="cityDto">Database city entity</param>
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
            PointsOfInterest = [..sourceCity.PointsOfInterest
                .Select(x => (PointOfInterest) x)]
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

    public static implicit operator PointOfInterestForUpdateDto(PointOfInterestDbEntity poiDbEnt) => new()
    {
        Name = poiDbEnt.Name,
        Description = poiDbEnt.Description
    };
}
public class PointOfInterestForCreatonDto
{
    [Required(ErrorMessage = "Name is required"), MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }

    public static implicit operator PointOfInterestDbEntity(PointOfInterestForCreatonDto poiForCreation) => new(poiForCreation.Name)
    {
        Description = poiForCreation.Description
    };
}
public class PointOfInterest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public static implicit operator PointOfInterest(PointOfInterestDbEntity dbEnt) => new()
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