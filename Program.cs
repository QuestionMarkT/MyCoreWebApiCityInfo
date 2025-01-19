global using Microsoft.AspNetCore.Mvc;
global using System;
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

namespace MyCoreWebApiCityInfo;

public class Program
{
    static void DumbPlaygroundArea()
    {
        if(0 == 0.0)
        {
            Console.WriteLine("Same");
        }
        if(0 == 0M)
        {
            Console.WriteLine("Same again");
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    public static void Main(string[] args)
    {
        //DumbPlaygroundArea();
        //return;
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.
            AddControllers(opts =>
            {
                //opts.InputFormatters.Add()
                opts.ReturnHttpNotAcceptable = true;
            })
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
            });

        WebApplication app = builder.Build();

        if(app.Environment.IsDevelopment())
        {
            app.UseSwagger().UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
#pragma warning disable ASP0014
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
