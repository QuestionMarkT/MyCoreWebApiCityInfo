using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCoreWebApiCityInfo.Entities;
using MyCoreWebApiCityInfo.DbContexts;
using System.Threading.Tasks;

namespace MyCoreWebApiCityInfo.Services;

public interface ICityInfoRepository
{
    IAsyncEnumerable<CityDbEntity> GetCities();
    Task<CityDbEntity?> GetCity(int cityId, bool includePoi = false);
    Task<bool> CityExists(int cityId);
    IAsyncEnumerable<PointOfInterestDbEntity> GetPointsOfInterestsForCity(int cityId);
    Task<PointOfInterestDbEntity?> GetPointOfInterestForCity(int cityId, int poiId);
    Task AddPointOfInterestForCity(int cityId, PointOfInterestDbEntity poiDbEntity);
    Task<bool> SaveChanges();
}

public interface IMail
{
    void Send(string subject, string body);
}

public class CityInfoRepository(CityInfoContext context) : ICityInfoRepository
{
    readonly CityInfoContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    public IAsyncEnumerable<CityDbEntity> GetCities() => _context.Cities.OrderBy(x => x.Name).AsAsyncEnumerable();

    public async Task<CityDbEntity?> GetCity(int cityId, bool includePoi = false)
    {
        if(includePoi)
            return await _context.Cities
                .Include(x => x.PointsOfInterest)
                .Where(x => x.Id == cityId)
                .FirstOrDefaultAsync();
        else
            return await _context.Cities
                .Where(x => x.Id == cityId)
                .FirstOrDefaultAsync();
    }

    public async Task<PointOfInterestDbEntity?> GetPointOfInterestForCity(int cityId, int poiId) => await _context.PointsOfInterest
        .Where(x => x.CityId == cityId && x.Id == poiId)
        .FirstOrDefaultAsync();

    public IAsyncEnumerable<PointOfInterestDbEntity> GetPointsOfInterestsForCity(int cityId) => _context.PointsOfInterest
        .Where(x => x.City != null && x.City.Id == cityId)
        .AsAsyncEnumerable();

    public async Task<bool> CityExists(int cityId) => await _context.Cities.AnyAsync(x => x.Id == cityId);

    public async Task AddPointOfInterestForCity(int cityId, PointOfInterestDbEntity poiDbEntity) => (await GetCity(cityId))?.PointsOfInterest.Add(poiDbEntity);

    public async Task<bool> SaveChanges() => await _context.SaveChangesAsync() >= 0;
}

public class LocalMail(IConfiguration config) : IMail
{
    readonly string _mailTo = config["mailSettings:mailToAddress"] ?? throw new ArgumentNullException(nameof(config));
    readonly string _mailFrom = config["mailSettings:mailFromAddress"] ?? throw new ArgumentNullException(nameof(config));

    public void Send(string subject, string body)
    {
        string message = $"Mail from {_mailFrom} to {_mailTo} with {nameof(LocalMail)}{Environment.NewLine}";
        message += "Subject: " + subject + Environment.NewLine;
        message += "Message: " + body;

        Console.WriteLine(message);
    }
}

public class CloudMail(IConfiguration config) : IMail
{
    readonly string _mailTo = config["mailSettings:mailToAddress"] ?? throw new ArgumentNullException(nameof(config));
    readonly string _mailFrom = config["mailSettings:mailFromAddress"] ?? throw new ArgumentNullException(nameof(config));

    public void Send(string subject, string body)
    {
        string message = $"Mail from {_mailFrom} to {_mailTo} with {nameof(CloudMail)}{Environment.NewLine}";
        message += "Subject: " + subject + Environment.NewLine;
        message += "Message: " + body;

        Console.WriteLine(message);
    }
}