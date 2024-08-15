using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationApi.Queries;

public interface IFlightQueries
{
    /// <summary>Get a specific flight by its database ID</summary>
    Task<FlightResponse> GetFlight(int flightID);

    // TODO GetAllFlights()
    // TODO FindFlight(FlightSearchOptions options)
}

public class FlightQueries : IFlightQueries
{
    private readonly ReadContext _db;
    public FlightQueries(ReadContext db)
    {
        _db = db;
    }

    public async Task<FlightResponse> GetFlight(int flightID)
    {
        return await _db.Flights
            .Where(f => f.FlightID == flightID)
            .Select(MapFromFlight)
            .FirstOrDefaultAsync();
    }

    private Expression<Func<Flight, FlightResponse>> MapFromFlight = (Flight flight) =>
        new FlightResponse {
            FlightID = flight.FlightID,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline,
            DepartureAirport = flight.DepartureAirport.Code,
            ArrivalAirport = flight.ArrivalAirport.Code,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Status = flight.Status,
            Version = flight.Version
        };

    private static FlightResponse MapFromFlight2(Flight flight)
    {
        return new FlightResponse
        {
            FlightID = flight.FlightID,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline,
            DepartureAirport = flight.DepartureAirport.Code,
            ArrivalAirport = flight.ArrivalAirport.Code,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Status = flight.Status,
            Version = flight.Version
        };
    }
}