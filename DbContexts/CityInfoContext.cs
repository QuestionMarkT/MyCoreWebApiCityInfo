#pragma warning disable CS1591
using MyCoreWebApiCityInfo.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace MyCoreWebApiCityInfo.DbContexts;

public class CityInfoContext(DbContextOptions<CityInfoContext> options, IConfiguration _config) : DbContext(options)
{
    readonly IConfiguration configuration = _config ?? throw new ArgumentNullException(nameof(_config));
    public DbSet<CityDbEntity> Cities { get; set; }
    public DbSet<PointOfInterestDbEntity> PointsOfInterest { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityDbEntity>()
            .HasData(
            new("New York City")
            {
                Id = 1,
                Description = "The one with that big park."
            },
            new("Antwerp")
            {
                Id = 2,
                Description = "The one with the cathedral that was never really finished."
            },
            new("Paris")
            {
                Id = 3,
                Description = "The one with that big tower."
            });

        modelBuilder.Entity<PointOfInterestDbEntity>()
            .HasData(
            new("Central Park")
            {
                Id = 1,
                CityId = 1,
                Description = "The most visited urban park in the United States."
            },
            new("Empire State Building")
            {
                Id = 2,
                CityId = 1,
                Description = "A 102-story skyscraper located in Midtown Manhattan."
            },
            new("Cathedral")
            {
                Id = 3,
                CityId = 2,
                Description = "A Gothic style cathedral, conceived by architects Jan and Pieter Appelmans."
            },
            new("Antwerp Central Station")
            {
                Id = 4,
                CityId = 2,
                Description = "The the finest example of railway architecture in Belgium."
            },
            new("Eiffel Tower")
            {
                Id = 5,
                CityId = 3,
                Description = "A wrought iron lattice tower on the Champ de Mars, named after engineer Gustave Eiffel."
            },
            new("The Louvre")
            {
                Id = 6,
                CityId = 3,
                Description = "The world's largest museum."
            });

        base.OnModelCreating(modelBuilder);
    }

    // OTHER WAY OF CREATING A DBCONTEXT CONNECTION
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(optionsBuilder.IsConfigured)
            Console.WriteLine("Connection string is already configured");
        else
            optionsBuilder.UseSqlite(configuration.GetConnectionString("CityInfoDBConnectionString"));
        
    }
}