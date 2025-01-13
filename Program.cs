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
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        builder.Services.
            AddControllers().Services.
            AddEndpointsApiExplorer().
            AddSwaggerGen();
        
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
            await context.Response.WriteAsync("404 page not found :(");
        });

        app.Run();
    }
}
