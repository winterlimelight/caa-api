using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using FlightInformationApi.Queries;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlightInformationApi.Tests.ContractTests;

/// <summary>Contract tests covering FlightInformationController.GetAll() and FlightInformationController.GetFlight()</summary>
public class FlightInformationControllerGetFlightTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public FlightInformationControllerGetFlightTests()
    {
        _factory = new CustomWebApplicationFactory<Program>();
    }

    void IDisposable.Dispose()
    {
        _factory?.Dispose();
    }

    private readonly Flight _testFlight1 = new Flight
    {
        FlightID = 1,
        FlightNumber = "QFA32",
        Airline = "Qantas",
        DepartureAirport = new Airport { Code = "WSSS", Name = "Singapore Changi" },
        ArrivalAirport = new Airport { Code = "YSSY", Name = "Sydney" },
        DepartureTime = new DateTimeOffset(2010, 10, 4, 1, 56, 47, TimeSpan.Zero),
        ArrivalTime = new DateTimeOffset(2010, 10, 4, 3, 46, 47, TimeSpan.Zero),
        Status = FlightStatus.Landed
    };

    private readonly Flight _testFlight2 = new Flight
    {
        FlightID = 2,
        FlightNumber = "AAL96",
        Airline = "American Airlines",
        DepartureAirport = new Airport { Code = "KDTW", Name = "Detroit Metropolitan" },
        ArrivalAirport = new Airport { Code = "KBUF", Name = "Buffalo Niagara" },
        DepartureTime = new DateTimeOffset(1972, 6, 13, 0, 20, 0, TimeSpan.Zero),
        ArrivalTime = new DateTimeOffset(1972, 6, 13, 0, 44, 0, TimeSpan.Zero),
        Status = FlightStatus.Landed
    };

    
    [Fact]
    public async Task GetSingleFlight()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Flights.AddRange(_testFlight1, _testFlight2);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var flight = await TestHelpers.ReadBody<FlightResponse>(response);
        Assert.Equal("QFA32", flight.FlightNumber);
        Assert.Equal("Qantas", flight.Airline);
        Assert.Equal("WSSS", flight.DepartureAirport);
        Assert.Equal("YSSY", flight.ArrivalAirport);
        Assert.Equal(new DateTimeOffset(2010, 10, 4, 1, 56, 47, TimeSpan.Zero), flight.DepartureTime);
        Assert.Equal(new DateTimeOffset(2010, 10, 4, 3, 46, 47, TimeSpan.Zero), flight.ArrivalTime);
        Assert.Equal(FlightStatus.Landed, flight.Status);
    }

    [Fact]
    public async Task GetSingleFlight_NotFound()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Flights.AddRange(_testFlight1, _testFlight2);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/3");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task GetAllFlights()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Flights.AddRange(_testFlight1, _testFlight2);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Equal(2, flights.Length);
        
        var flight1 = flights.Single(f => f.FlightNumber == "QFA32");
        var flight2 = flights.Single(f => f.FlightNumber == "AAL96");

        // sampling of fields only
        Assert.Equal("WSSS", flight1.DepartureAirport);
        Assert.Equal(new DateTimeOffset(2010, 10, 4, 1, 56, 47, TimeSpan.Zero), flight1.DepartureTime);
        Assert.Equal("American Airlines", flight2.Airline);
        Assert.Equal(FlightStatus.Landed, flight2.Status);
    }

    [Fact]
    public async Task GetAllFlights_Empty()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Flights.AddRange();
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Empty(flights);
    }
}