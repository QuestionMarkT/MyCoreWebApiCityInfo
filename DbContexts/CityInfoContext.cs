using MyCoreWebApiCityInfo.Entities;
using Microsoft.EntityFrameworkCore;

namespace MyCoreWebApiCityInfo.DbContexts;

public class CityInfoContext : DbContext
{
    public DbSet<City> Cities { get; set; }
    public DbSet<PointOfInterest> PointsOfInterest { get; set; }

    // METHOD #1 CREATING A DBCONTEXT CONNECTION
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=CityInfo.db");
        base.OnConfiguring(optionsBuilder);
    }
}