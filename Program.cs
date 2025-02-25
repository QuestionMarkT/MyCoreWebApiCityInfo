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

namespace MyCoreWebApiCityInfo;

public class Program
{
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
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
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
            .AddDbContext<CityInfoContext>();
            //.AddDbContext<CityInfoContext>(dbco =>
            //{
            //    dbco.UseSqlite();
            //});

        using WebApplication app = builder.Build();

        if(!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
        }

        if(app.Environment.IsDevelopment())
        {
            app.UseSwagger().UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
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