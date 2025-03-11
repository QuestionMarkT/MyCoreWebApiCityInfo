using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCoreWebApiCityInfo.Entities;
using MyCoreWebApiCityInfo.Models;
using MyCoreWebApiCityInfo.Services;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.IO.File;

namespace MyCoreWebApiCityInfo.Controllers;

[Route("api/[controller]"), ApiController, Authorize]
public class CitiesController(ICityInfoRepository __cityInfoRepository) : ControllerBase
{
    readonly ICityInfoRepository _ciRepo = __cityInfoRepository ??
        throw new ArgumentNullException(nameof(__cityInfoRepository));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CityWithoutPoi>>> GetCities(
        [FromQuery(Name = "name")] string? cityNameFromUser,
        [FromQuery(Name = "search")] string? searchFromUser,
        [FromQuery(Name = "page")] int pageFromUser = 1,
        [FromQuery(Name = "pageSize")] int pageSizeFromUser = 25)
    {
        List<CityWithoutPoi> result = [];
        pageSizeFromUser = int.Clamp(pageSizeFromUser, 1, 100);

        var (answer, pMeta) = await _ciRepo.GetCities(cityNameFromUser, searchFromUser, pageFromUser, pageSizeFromUser);

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pMeta));

        await foreach(CityDbEntity? city in answer)
            result.Add((CityWithoutPoi) city);

        return result.Count > 0 ? Ok(result) : NoContent();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCity(int id, [FromQuery] bool includePoi = false)
    {
        CityDbEntity? cityDbEntity =  await _ciRepo.GetCity(id, includePoi);
        
        if(cityDbEntity is null)
            return NotFound("404 city not found");
        else if(includePoi)
            return Ok((City) cityDbEntity);
        else
            return Ok((CityWithoutPoi) cityDbEntity);
    }
}

[Route("api/[controller]"), ApiController, Authorize]
public class FilesController(FileExtensionContentTypeProvider fectp) : ControllerBase
{
    readonly FileExtensionContentTypeProvider _fectp = fectp ?? throw new ArgumentNullException(nameof(fectp) + " is null in FilesController class");

    [HttpGet("{fileId}")]
    public ActionResult GetFile(string fileId)
    {
#if true
        string filePath = "blank PDF.pdf";
#else
        string filePath = "blank document no pw.pdf";
#endif
        if(!Exists(filePath))
            return NotFound();

        if(!_fectp.TryGetContentType(filePath, out string? contentType))
            contentType = "application/octet-stream";

        byte[] fileBytes = ReadAllBytes(filePath);
        return File(fileBytes, contentType, Path.GetFileName(filePath));
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateFile(IFormFile file)
    {
        if(file.Length == 0 || file.Length > 100 * 1024 * 1024 || file.ContentType != "application/pdf")
        {
            return BadRequest("No file or an invalid one has been provided");
        }

        string newFilePath = Path.Join(Directory.GetCurrentDirectory(), $"uploaded file {Guid.NewGuid()}.pdf");
        using FileStream fstream = new(newFilePath, FileMode.Create);
        CancellationTokenSource ct = new(TimeSpan.FromMinutes(5));
        await file.CopyToAsync(fstream, ct.Token);

        return Ok("File has been uploaded");
    }
}

[Route("api/cities/{cityId}/[controller]"), ApiController, Authorize(Program.CityPolicy)]
public class PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMail localMail, ICityInfoRepository __cds) : ControllerBase
{
    #region fields
    readonly ILogger<PointsOfInterestController> _logger = logger ??
        throw new ArgumentNullException(nameof(logger));

    readonly IMail _mailSrv = localMail ??
        throw new ArgumentNullException(nameof(localMail));

    readonly ICityInfoRepository _citiesDatabase = __cds ??
        throw new ArgumentNullException(nameof(__cds));

    const string poiRoute = nameof(GetPointOfInterest);
    #endregion

    #region methods
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> GetPointsOfInterest(int cityId)
    {
        /* REPLACED WITH AUTHORIZE POLICY
        string? cityName = User.Claims.FirstOrDefault(x => x.Type == "city")?.Value;
        
        if(!await _citiesDatabase.CityNameMatchesCityId(cityName, cityId))
            return Forbid();*/

        if(!await _citiesDatabase.CityExists(cityId))
        {
            //Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5, None = 6
            _logger.LogInformation("City ID {cityId} wasn't found when accessing points of interest.", cityId);
            return NotFound($"404 city ID {cityId} not found");
        }

        List<PointOfInterest> result = [];
        
        await foreach(PointOfInterestDbEntity poiDbEnt in _citiesDatabase.GetPointsOfInterestsForCity(cityId))
            result.Add(poiDbEnt);

        return Ok(result);
    }

    [HttpGet($"{{{nameof(pointOfInterestId)}}}", Name = poiRoute)]
    public async Task<ActionResult<PointOfInterest>> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        PointOfInterestDbEntity? poiDbEnt = await _citiesDatabase.GetPointOfInterestForCity(cityId, pointOfInterestId);

        if(poiDbEnt is null)
            return NotFound("404 point of interest or city not found");
        
        return Ok((PointOfInterest)poiDbEnt);
    }

    [HttpPost]
    public async Task<ActionResult<PointOfInterest>> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreatonDto poiForCreation)
    {
        if(!await _citiesDatabase.CityExists(cityId))
            return NotFound();

        PointOfInterestDbEntity poiDbEnt = poiForCreation;

        await _citiesDatabase.AddPointOfInterestForCity(cityId, poiDbEnt);
        await _citiesDatabase.SaveChanges();
        

        PointOfInterest poi = poiDbEnt;

        return CreatedAtRoute(poiRoute,
            new
            {
                cityId,
                pointOfInterestId = poi.Id
            },
            poi);
    }

    [HttpPut($"{{{nameof(poiIdFromUser)}}}")]
    public async Task<ActionResult> UpdatePointOfInterest(int cityId, int poiIdFromUser, PointOfInterestForUpdateDto poiFromUser)
    {
        if(!await _citiesDatabase.CityExists(cityId))
            return NotFound($"404 city {cityId} not found");

        PointOfInterestDbEntity? poiDbEnt = await _citiesDatabase.GetPointOfInterestForCity(cityId, poiIdFromUser);

        if(poiDbEnt is null)
            return NotFound($"404 point of interest for city ID {cityId} not found");

        poiDbEnt.Description = poiFromUser.Description;
        poiDbEnt.Name = poiFromUser.Name;
        await _citiesDatabase.SaveChanges();

        return NoContent();
    }

    [HttpPatch($"{{{nameof(poiId)}}}")]
    public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        if(!await _citiesDatabase.CityExists(cityId))
            return NotFound($"404 city ID {cityId} not found");

        PointOfInterestDbEntity? poiDbEnt = await _citiesDatabase.GetPointOfInterestForCity(cityId, poiId);
        if(poiDbEnt is null)
            return NotFound($"404 point of interest ID {poiId} not found");

        PointOfInterestForUpdateDto poiToPatch = poiDbEnt;
        patchDocument.ApplyTo(poiToPatch, ModelState);

        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        if(!TryValidateModel(poiToPatch))
            return BadRequest(ModelState);

        poiDbEnt.Update(poiToPatch);
        await _citiesDatabase.SaveChanges();

        return NoContent(); // 204 success
    }

    [HttpDelete($"{{{nameof(poiId)}}}")]
    public async Task<IActionResult> DeletePointOfInterest(int cityId, int poiId)
    {
        if(!await _citiesDatabase.CityExists(cityId))
            return NotFound($"404 city ID {cityId} not found");

        PointOfInterestDbEntity? poiEnt = await _citiesDatabase.GetPointOfInterestForCity(cityId, poiId);
        if(poiEnt is null)
            return NotFound($"404 point of interest ID {poiId} not found");
        
        _citiesDatabase.DeletePointOfInterest(poiEnt);
        await _citiesDatabase.SaveChanges();
        
        _mailSrv.Send("Poi deleted", $"Point of interest {poiEnt.Name} with id {poiEnt.Id} has been deleted.");
        return NoContent();
    }
    #endregion
}

[Route("api/[controller]"), ApiController]
public class AuthenticationController(IConfiguration __config) : ControllerBase
{
    readonly IConfiguration _config = __config ?? throw new ArgumentNullException(nameof(__config));
    
    public class AuthenticationRequestBody
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    class CityInfoUser(int userId, string username, string firstName, string lastName, string city)
    {
        public int UserId { get; set; } = userId;
        public string Username { get; set; } = username;
        public string FirstName { get; set; } = firstName;
        public string LastName { get; set; } = lastName;
        public string City { get; set; } = city;
    }
    
    [HttpPost(nameof(Authenticate))]
    public ActionResult<string> Authenticate(AuthenticationRequestBody authReqBody)
    {
        CityInfoUser user = ValidateUserCredentials(authReqBody.Username, authReqBody.Password);
        
        if(user is null)
            return Unauthorized();
        
        string? authSecretFkey = _config["Authentication:SecretForKey"];
        string? authIssuer = _config["Authentication:Issuer"];
        string? authAudience = _config["Authentication:Audience"];
        
        if(authIssuer.IsNows(authAudience, authSecretFkey))
            return StatusCode(500, "config null value");

        SymmetricSecurityKey securityKey = new(Convert.FromBase64String(authSecretFkey!));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);
        Claim[] claimsForToken = 
        [
            new("sub", user.UserId.ToString()),
            new("given_name", user.FirstName),
            new("family_name", user.LastName),
            new("city", user.City)
        ];
        
        JwtSecurityToken jsonWebToken = new(
            authIssuer,
            authAudience,
            claimsForToken,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(2),
            signingCredentials);

        string tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jsonWebToken);
        return Ok(tokenToReturn);
    }

    static CityInfoUser ValidateUserCredentials(string? username, string? password)
    {
        // no users database, assuming this comes from a DB
        if(username == "KevinDockx" && password is not null && password.Contains("long"))
            return new(1, username ?? "", "Kevin", "Dockx", "Antwerp");
        else
            return null!;
    }
}