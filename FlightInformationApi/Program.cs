using FlightInformationApi.Commands;
using FlightInformationApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlightInformationApi;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ConfigureDatabase(builder);
        DependencyInjection(builder);

        var app = builder.Build();

        using (var serviceScope = app.Services.CreateScope())
        {
            var db = serviceScope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Database.EnsureCreated();
            
            // TODO initial db population
            db.Airports.Add(new Airport { AirportID = 1, Code = "NZAA", Name = "Auckland International Airport" });
            db.SaveChanges();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        string connStr = builder.Configuration.GetConnectionString("main");
        builder.Services.AddDbContext<ReadContext>(opt => opt.UseSqlite(connStr));
        builder.Services.AddDbContext<WriteContext>(opt => opt.UseSqlite(connStr));
    }

    // dependency injection - ideally we'd use AutoFac or write something here to populate many of these automatically
    private static void DependencyInjection(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICommandHandler<SetFlightCommand, IdCommandResponse>, SetFlightCommandHandler>();

        builder.Services.AddTransient<IModelValidator, ModelValidator>();
    }
}
