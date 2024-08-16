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

/// <summary>Contract tests covering FlightInformationController.UpdateFlight()</summary>
/// <remarks>This uses the same request object as CreateFlight() input validation
/// tests are found in FlightInformationControllerCreateFlightTests</remarks>
public class FlightInformationControllerUpdateFlightTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory = new CustomWebApplicationFactory<Program>();
    private readonly Mock<ILogger<FlightInformationController>> _logger = new();

    private SetFlightCommand UpdateRequestFromFlight(Flight flight) => new SetFlightCommand
    {
        FlightNumber = flight.FlightNumber,
        Airline = flight.Airline,
        DepartureAirport = flight.DepartureAirport.Code,
        ArrivalAirport = flight.ArrivalAirport.Code,
        DepartureTime = flight.DepartureTime,
        ArrivalTime = flight.ArrivalTime,
        Status = flight.Status,
        Version = flight.Version
    };

    void IDisposable.Dispose()
    {
        _factory?.Dispose();
    }

    [Fact]
    public async Task UpdateFlight_Success()
    {
        HttpClient client = CreateClientWithLogger();
        Flight originalFlight = await CreateTestFlight();

        SetFlightCommand request = UpdateRequestFromFlight(originalFlight);

        request.ArrivalAirport = "LPLA";
        request.ArrivalTime = new DateTimeOffset(2001, 8, 24, 6, 45, 0, TimeSpan.Zero);
        request.Status = FlightStatus.Landed;

        var response = await client.PutAsync("/api/flights/" + originalFlight.FlightID, TestHelpers.ToJsonBody(request));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var flight = GetFlightFromDatabase();
        Assert.Equal("CYYZ", flight.DepartureAirport.Code);
        Assert.Equal("LPLA", flight.ArrivalAirport.Code);
        Assert.Equal(new DateTimeOffset(2001, 8, 24, 0, 52, 0, TimeSpan.Zero), flight.DepartureTime);
        Assert.Equal(new DateTimeOffset(2001, 8, 24, 6, 45, 0, TimeSpan.Zero), flight.ArrivalTime);
        Assert.Equal(FlightStatus.Landed, flight.Status);
        Assert.NotEqual(originalFlight.Version, flight.Version);

        VerifyLogger(LogLevel.Trace, "UpdateFlight()", Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateFlight_NotFoundFlightID()
    {
        HttpClient client = CreateClientWithLogger();
        Flight originalFlight = await CreateTestFlight();

        SetFlightCommand request = UpdateRequestFromFlight(originalFlight);
        var response = await client.PutAsync("/api/flights/14", TestHelpers.ToJsonBody(request));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateFlight_InvalidFlightID(int flightID)
    {
        HttpClient client = CreateClientWithLogger();
        Flight originalFlight = await CreateTestFlight();

        var response = await client.PutAsync("/api/flights/" + flightID, TestHelpers.ToJsonBody(UpdateRequestFromFlight(originalFlight)));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFlight_WrongVersion()
    {
        HttpClient client = CreateClientWithLogger();
        Flight originalFlight = await CreateTestFlight();

        SetFlightCommand request = UpdateRequestFromFlight(originalFlight);
        request.Status = FlightStatus.Landed;
        request.Version = Guid.NewGuid();

        var response = await client.PutAsync("/api/flights/" + originalFlight.FlightID, TestHelpers.ToJsonBody(request));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

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

    private async Task<Flight> CreateTestFlight()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();

            Flight testFlight1 = new Flight
            {
                FlightID = 1,
                FlightNumber = "TSC236",
                Airline = "Air Transat",
                DepartureAirport = new Airport { Code = "CYYZ", Name = "Toronto" },
                ArrivalAirport = new Airport { Code = "LPPT", Name = "Lisbon" },
                DepartureTime = new DateTimeOffset(2001, 8, 24, 0, 52, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2001, 8, 24, 8, 0, 0, TimeSpan.Zero),
                Status = FlightStatus.InAir,
                Version = Guid.NewGuid()
            };

            db.Flights.AddRange(testFlight1);
            db.Airports.Add(new Airport { Code = "LPLA", Name = "Lajes Airport" });
            await db.SaveChangesAsync();
            return testFlight1;
        }
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