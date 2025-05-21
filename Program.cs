#pragma warning disable CS1591 // missing XML comment for publicly visible type or member
global using Microsoft.AspNetCore.Mvc;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using MyCoreWebApiCityInfo.DbContexts;
using MyCoreWebApiCityInfo.Services;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.OpenApi.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Asp.Versioning.ApiExplorer;

namespace MyCoreWebApiCityInfo;

public class Program
{
    public const string CityPolicy = "MustBeFromAntwerp";
    #region unrelated testing playground area
    static void DumbPlaygroundArea()
    {

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
    #endregion
    public static void Main(string[] args)
    {
        //DumbPlaygroundArea();
        //return;
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/city info.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();
        builder.Services.AddControllers(opts =>
            {
                //opts.InputFormatters.Add()
                opts.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters()
            .Services
            .AddEndpointsApiExplorer() // exposes available endpoints and how to interact with them, used by Swashbuckle to generate the OpenAPI specification
            .AddSwaggerGen(opts => // registers services used for generating spec
            {
                string xmlCommentsFilePath = Path.Join(AppContext.BaseDirectory, "WebApiDocs.xml");

                if(!File.Exists(xmlCommentsFilePath))
                    throw new FileNotFoundException("WebApiDocs.xml wasn't found: " + xmlCommentsFilePath);

                opts.IncludeXmlComments(xmlCommentsFilePath, true);

                opts.AddSecurityDefinition("CityInfoApiBearerAuth", new()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    Description = "Input a valid token to access this API"
                });

                opts.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    [
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "CityInfoApiBearerAuth"
                            }
                        }
                    ] = []
                });
            })
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional info example");
                    ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);
                };
            })
            .AddSingleton<FileExtensionContentTypeProvider>()
#if DEBUG
            .AddTransient<IMail, LocalMail>()
#else
            .AddTransient<IMail, CloudMail>()
#endif
            .AddSingleton<CitiesDataStore>()
            .AddDbContext<CityInfoContext>()
            .AddScoped<ICityInfoRepository, CityInfoRepository>()
            .AddAuthentication("Bearer")
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero, // by default it's 5 minutes
                    ValidIssuer = builder.Configuration["Authentication:Issuer"],
                    ValidAudience = builder.Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"] ?? throw new NullReferenceException("SecretForKey")))
                };
            })
            .Services
            .AddAuthorizationBuilder()
            .AddPolicy(CityPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("city", "Antwerp");
            })
            .Services
            .AddApiVersioning(opts =>
            {
                opts.ReportApiVersions = true;
                opts.AssumeDefaultVersionWhenUnspecified = true;
                opts.DefaultApiVersion = new(1, 0);
            })
            .AddMvc()
            .AddApiExplorer(setup =>
            {
                setup.SubstituteApiVersionInUrl = true;
                //setup.GroupNameFormat = "'v'VVV";
            });
        
        IApiVersionDescriptionProvider apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

        builder.Services.AddSwaggerGen(opts =>
        {
            foreach(ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                opts.SwaggerDoc(description.GroupName, new()
                {
                    Title = "City Info API",
                    Version = description.ApiVersion.ToString(),
                    Description = "Through this API you can access cities and their points of interest."
                });
            }
        });

        using WebApplication app = builder.Build();
        
        if(!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
        }
        
        if(app.Environment.IsDevelopment())
        {
            app.UseSwagger() // ensures the middleware for generating the OpenAPI specification is added
                .UseSwaggerUI(setupAction =>
                {
                    IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();

                    foreach(ApiVersionDescription description in descriptions)
                    {
                        setupAction.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                        setupAction.RoutePrefix = string.Empty; // makes the Swagger UI available at the app's root URL
                    }
                }); // ensures the middleware that uses that specification to generate the default SwaggerUI documentation URI gets added
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
#pragma warning disable ASP0014 // idk why but in the course it is at it is
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
#pragma warning restore ASP0014
        app.Run(async (context) =>
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("404 page not found :(");
        });
        app.Run();
    }
}

public static class Exts
{
    extension(string? arg1)
    {
        /// <summary>
     /// Shorthand for string.IsNullOrWhiteSpace()
     /// </summary>
     /// <param name="args">additional strings to check</param>
     /// <returns>true if it is</returns>
        public bool IsNows(params string?[] args)
        {
            if(string.IsNullOrWhiteSpace(arg1))
                return true;

            foreach(string? arg in args)
                if(string.IsNullOrWhiteSpace(arg))
                    return true;

            return false;
        }
    }
}