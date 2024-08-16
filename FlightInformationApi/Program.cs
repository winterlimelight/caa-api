using System;
using System.Text.Json.Serialization;
using FlightInformationApi.Commands;
using FlightInformationApi.Data;
using FlightInformationApi.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlightInformationApi;


public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        IMvcBuilder mvcBuilder = builder.Services.AddControllers();
        mvcBuilder.AddJsonOptions(opts => {
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opts =>
        {
            opts.IncludeXmlComments(System.Reflection.Assembly.GetExecutingAssembly());
            opts.UseInlineDefinitionsForEnums();
        });

        ConfigureDatabase(builder);
        DependencyInjection(builder);

        var app = builder.Build();

        using (var serviceScope = app.Services.CreateScope())
        {
            var db = serviceScope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Database.EnsureCreated();
            PopulateDatabase(db);
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

        builder.Services.AddScoped<IFlightQueries, FlightQueries>();

        builder.Services.AddTransient<IModelValidator, ModelValidator>();
    }

    private static void PopulateDatabase(WriteContext db)
    {
        db.Airports.Add(new Airport { AirportID = 1, Code = "NZAA", Name = "Auckland" });
        db.Airports.Add(new Airport { AirportID = 2, Code = "NZCH", Name = "Christchurch" });
        db.Airports.Add(new Airport { AirportID = 3, Code = "NZDN", Name = "Dunedin" });
        db.Airports.Add(new Airport { AirportID = 4, Code = "NZHN", Name = "Hamilton" });
        db.Airports.Add(new Airport { AirportID = 5, Code = "NZOH", Name = "Ohakea (MIL)" });
        db.Airports.Add(new Airport { AirportID = 6, Code = "NZPM", Name = "Palmerston North" });
        db.Airports.Add(new Airport { AirportID = 7, Code = "NZQN", Name = "Queenstown" });
        db.Airports.Add(new Airport { AirportID = 8, Code = "NZWN", Name = "Wellington" });
    }
}
