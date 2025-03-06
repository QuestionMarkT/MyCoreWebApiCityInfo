using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCoreWebApiCityInfo.Entities;
using MyCoreWebApiCityInfo.DbContexts;
using System.Threading.Tasks;

namespace MyCoreWebApiCityInfo.Services;

public interface ICityInfoRepository
{
    Task<(IAsyncEnumerable<CityDbEntity>, PaginationMetadata)> GetCities(string? name = "", string? search = "", int pageNumber = 1, int pageSize = 25);
    Task<CityDbEntity?> GetCity(int cityId, bool includePoi = false);
    Task<bool> CityExists(int cityId);
    IAsyncEnumerable<PointOfInterestDbEntity> GetPointsOfInterestsForCity(int cityId);
    Task<PointOfInterestDbEntity?> GetPointOfInterestForCity(int cityId, int poiId);
    Task AddPointOfInterestForCity(int cityId, PointOfInterestDbEntity poiDbEntity);
    void DeletePointOfInterest(PointOfInterestDbEntity poi);
    Task<bool> SaveChanges();
}

public interface IMail
{
    void Send(string subject, string body);
}

public class CityInfoRepository(CityInfoContext context) : ICityInfoRepository
{
    //const StringComparison sComp = StringComparison.OrdinalIgnoreCase;
    readonly CityInfoContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IAsyncEnumerable<CityDbEntity>, PaginationMetadata)> GetCities(string? name = "", string? search = "", int pageNumber = 1, int pageSize = 25)
    {
        var collection = _context.Cities as IQueryable<CityDbEntity>;

        if(!string.IsNullOrWhiteSpace(name))
            collection = collection.Where(x => EF.Functions.Like(x.Name, $"{name}"));
        
        if(!string.IsNullOrWhiteSpace(search))
            collection = collection.Where(x => 
                EF.Functions.Like(x.Name, $"{name}") ||
                (x.Description != null && EF.Functions.Like(x.Description, $"%{search}%")));
        
        int totalItemCount = await collection.CountAsync();
        PaginationMetadata pMeta = new(totalItemCount, pageSize, pageNumber);

        return (collection
            .OrderBy(x => x.Name)
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .AsAsyncEnumerable(), pMeta);
    }

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

    public void DeletePointOfInterest(PointOfInterestDbEntity poi) => _context.PointsOfInterest.Remove(poi);
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

public class PaginationMetadata(int totalItemCount, int pageSize, int currentPage)
{
    public int TotalItemCount { get; set; } = totalItemCount;
    public int TotalPageCount { get; set; } = (int) Math.Ceiling(totalItemCount / (double) pageSize);
    public int PageSize { get; set; } = pageSize;
    public int CurrentPage { get; set; } = currentPage;
}