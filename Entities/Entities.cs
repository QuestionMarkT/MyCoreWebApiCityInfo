#pragma warning disable CS1591
using MyCoreWebApiCityInfo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCoreWebApiCityInfo.Entities;

public class CityDbEntity(string name)
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false), MaxLength(80)]
    public string Name { get; set; } = name;

    [MaxLength(200)]
    public string? Description { get; set; }
    public ICollection<PointOfInterestDbEntity> PointsOfInterest { get; set; } = [];
}

public class PointOfInterestDbEntity(string name)
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false), MaxLength(80)]
    public string Name { get; set; } = name;

    [MaxLength(200)]
    public string? Description { get; set; }

    [ForeignKey(nameof(CityId))]
    public CityDbEntity? City { get; set; }
    public int CityId { get; set; }

    public void Update(PointOfInterestForUpdateDto updateModel)
    {
        Name = updateModel.Name;
        Description = updateModel.Description;
    }
}