﻿using System.ComponentModel.DataAnnotations;

namespace MyCoreWebApiCityInfo.Models;

public class PointOfInterestForCreatonDto
{
    [Required(ErrorMessage = "Name is required"), MaxLength(50)] // https://docs.fluentvalidation.net/en/latest/ for more advanced input validation
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }
}
