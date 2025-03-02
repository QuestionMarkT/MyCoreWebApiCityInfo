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
    public ICollection<PointOfInterestDBEntity> PointsOfInterest { get; set; } = [];
}

public class PointOfInterestDBEntity(string name)
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
}