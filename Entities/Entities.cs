using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCoreWebApiCityInfo.Entities;

public class City(string name)
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false), MaxLength(80)]
    public string Name { get; set; } = name;

    [MaxLength(200)]
    public string? Description { get; set; }
    public ICollection<PointOfInterest> PointsOfInterest { get; set; } = [];
}

public class PointOfInterest(string name)
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false), MaxLength(80)]
    public string Name { get; set; } = name;

    [MaxLength(200)]
    public string? Description { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }
    public int CityId { get; set; }
}