using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using FlightInformationApi.Commands;
using FlightInformationApi.Controllers;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FlightInformationApi.Tests.ContractTests;

/// <summary>Contract tests covering FlightInformationController.CreateFlight()</summary>
public class FlightInformationControllerCreateFlightTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory = new CustomWebApplicationFactory<Program>();
    private readonly Mock<ILogger<FlightInformationController>> _logger = new();

    private SetFlightCommand CreateRequest() => new SetFlightCommand
    {
        FlightNumber = "ANZ179M",
        Airline = "Air New Zealand",
        DepartureAirport = "NZPM",
        ArrivalAirport = "NZCH",
        DepartureTime = new DateTimeOffset(2024, 8, 16, 7, 5, 0, TimeSpan.Zero),
        ArrivalTime = new DateTimeOffset(2024, 8, 16, 8, 25, 0, TimeSpan.Zero),
        Status = FlightStatus.Scheduled
    };

    void IDisposable.Dispose()
    {
        _factory?.Dispose();
    }

    [Fact]
    public async Task CreateFlight_Success()
    {
        HttpClient client = CreateClientWithLogger();

        SetFlightCommand request = CreateRequest();
        var response = await client.PostAsync("/api/flights", TestHelpers.ToJsonBody(request));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/flights/1", response.Headers.Location.AbsolutePath);

        var flight = GetFlightFromDatabase();
        Assert.Equal("ANZ179M", flight.FlightNumber);
        Assert.Equal("Air New Zealand", flight.Airline);
        Assert.Equal("NZPM", flight.DepartureAirport.Code);
        Assert.Equal("NZCH", flight.ArrivalAirport.Code);
        Assert.Equal(new DateTimeOffset(2024, 8, 16, 7, 5, 0, TimeSpan.Zero), flight.DepartureTime);
        Assert.Equal(new DateTimeOffset(2024, 8, 16, 8, 25, 0, TimeSpan.Zero), flight.ArrivalTime);
        Assert.Equal(FlightStatus.Scheduled, flight.Status);

        VerifyLogger(LogLevel.Trace, "CreateFlight()", Times.Exactly(2));
    }

    [Fact]
    public async Task CreateFlight_InvalidStatus()
    {
        HttpClient client = CreateClientWithLogger();

        SetFlightCommand request = CreateRequest();
        request.Status = (FlightStatus)0;

        var response = await client.PostAsync("/api/flights", TestHelpers.ToJsonBody(request));
            
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Status enum must be a valid FlightStatus", body);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("ABCDE")]
    public async Task CreateFlight_InvalidDepartureAirportCode(string badCode)
    {
        HttpClient client = CreateClientWithLogger();

        SetFlightCommand request = CreateRequest();
        request.DepartureAirport = badCode;

        var response = await client.PostAsync("/api/flights", TestHelpers.ToJsonBody(request));
            
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Value must be 4 character ICAO airport identifier", body);
    }

    // unknown airport code

    // bad times (might need to create json manually for that)

    // flight num rules

    // required fields

    private HttpClient CreateClientWithLogger()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_logger.Object);
            });
        }).CreateClient();
    }

    // https://adamstorr.co.uk/blog/mocking-ilogger-with-moq/
    private void VerifyLogger(LogLevel level, string startsWith, Times times)
    {
        _logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith(startsWith)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ), times);
    }

    private Flight GetFlightFromDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
        return db.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .SingleOrDefault();
    }
}