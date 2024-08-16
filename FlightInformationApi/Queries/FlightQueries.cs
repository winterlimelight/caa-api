using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationApi.Queries;

public interface IFlightQueries
{
    /// <summary>Get all flights</summary>
    Task<List<FlightResponse>> GetAllFlights();
    /// <summary>Get a specific flight by its database ID</summary>
    Task<FlightResponse> GetFlight(int flightID);
    /// <summary>Search </summary>
    Task<List<FlightResponse>> SearchFlights(FlightSearchOptions options);
}

public class FlightQueries : IFlightQueries
{
    private readonly ReadContext _db;
    public FlightQueries(ReadContext db)
    {
        _db = db;
    }

    public async Task<List<FlightResponse>> GetAllFlights()
    {
        return await _db.Flights.Select(MapFromFlight).ToListAsync();
    }

    public async Task<FlightResponse> GetFlight(int flightID)
    {
        return await _db.Flights
            .Where(f => f.FlightID == flightID)
            .Select(MapFromFlight)
            .FirstOrDefaultAsync();
    }

    public async Task<List<FlightResponse>> SearchFlights(FlightSearchOptions options)
    {
        var results = new List<FlightResponse>();

        IQueryable<Flight> query = _db.Flights;

        if (!string.IsNullOrWhiteSpace(options.Airline))
            query = query.Where(f => f.Airline == options.Airline);

        if (!string.IsNullOrWhiteSpace(options.Airport))
            query = query.Where(f => f.DepartureAirport.Name.Contains(options.Airport)
                || f.ArrivalAirport.Name.Contains(options.Airport)
                || f.DepartureAirport.Code == options.Airport
                || f.ArrivalAirport.Code == options.Airport);

        results = await query.Select(MapFromFlight).ToListAsync();

        // SQLite does not support ordering of DateTimeOffset fields: https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
        // Had I known this earlier I'd have used DateTime instead and ensured they are UTC. However, given this is a 
        // demonstration only I will revert to using a client side filter. Obviously this wouldn't be ideal in production
        // as it means many results being returned unecessarily from the database.

        if(options.FromDate.HasValue)
            results = results.Where(f => f.DepartureTime > options.FromDate.Value).ToList();

        if(options.ToDate.HasValue)
            results = results.Where(f => f.ArrivalTime < options.ToDate.Value).ToList();

        return results;
    }

    private Expression<Func<Flight, FlightResponse>> MapFromFlight = (Flight flight) =>
        new FlightResponse
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