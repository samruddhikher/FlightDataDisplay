using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FlightDataDisplay.Application;
using FlightDataDisplay.Domain;
using FlightDataDisplay.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using System.IO;
using FlightDataDisplay.Pages;

namespace FlightDataDisplay.Presentation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);           

            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true);
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }
            builder.Configuration.AddEnvironmentVariables(prefix: "FLIGHTDATADISPLAY_");
            builder.Configuration.AddCommandLine(args);

            // Add Blazor Server services
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddSingleton<BaggageHandler>();
            builder.Services.AddSingleton<ArrivalsMonitor>(name => new ArrivalsMonitor("Main Terminal"));
            builder.Services.AddSingleton<IAirportResolver, AirportResolver>();
            builder.Services.AddHttpClient("OpenSky", client =>
           {
               client.Timeout = TimeSpan.FromSeconds(30);
           });
            builder.Services.AddSingleton<IFlightDataRepository>(sp =>
           {
               var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
               var airportResolver = sp.GetRequiredService<IAirportResolver>();
               var configuration = sp.GetRequiredService<IConfiguration>();

               var clientId = configuration["OpenSky:ClientId"];
               var clientSecret = configuration["OpenSky:ClientSecret"];
               var airportIcao = configuration["OpenSky:AirportIcao"] ?? "EDDF";

               return new OpenskyFlightData(httpClientFactory, airportResolver, clientId, clientSecret, airportIcao);
           });
            builder.Services.AddHostedService<ApplicationRunner>();
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
