using System;
using System.Linq;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationApi.Commands;

/// <summary>Create or update a flight from the requested flight command</summary>
public class SetFlightCommandHandler : ICommandHandler<SetFlightCommand, IdCommandResponse>
{
    private readonly WriteContext _db;
    private readonly IModelValidator _modelValidator;

    public SetFlightCommandHandler(WriteContext db, IModelValidator modelValidator)
    {
        _db = db;
        _modelValidator = modelValidator;
    }

    async public Task<IdCommandResponse> Execute(SetFlightCommand request)
    {
        // Invalid model should be caught at the controller level. This is defensive, hence a non-descriptive error message.
        if (!_modelValidator.Validate(request))
            throw new FlightInformationException("Invalid request.");

        // validate other elements of the request
        if (request.ArrivalTime < request.DepartureTime)
            throw new FlightInformationException("Arrival time must be after departure time.");

        var airports = await _db.Airports.Where(a => a.Code == request.ArrivalAirport || a.Code == request.DepartureAirport).ToListAsync();

        if (!airports.Any(a => a.Code == request.ArrivalAirport))
            throw new FlightInformationException($"Airport with code {request.ArrivalAirport} not found.");
        if (!airports.Any(a => a.Code == request.DepartureAirport))
            throw new FlightInformationException($"Airport with code {request.DepartureAirport} not found.");

        var flight = await _db.Flights.FirstOrDefaultAsync(f => f.FlightID == request.FlightID);
        if (request.FlightID == 0) // new flight
        {
            // create flight
            flight = new Flight
            {
                FlightNumber = request.FlightNumber,
                Airline = request.Airline,
                DepartureAirport = airports.Single(a => a.Code == request.DepartureAirport),
                ArrivalAirport = airports.Single(a => a.Code == request.ArrivalAirport),
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                Status = request.Status,
                Version = Guid.NewGuid()
            };
            _db.Flights.Add(flight);
        }
        else
        {
            // update existing flight
            if (flight == null)
                throw new NotFoundException();
            if (flight.Version != request.Version)
                throw new ModifiedException();

            flight.FlightNumber = request.FlightNumber;
            flight.Airline = request.Airline;
            flight.DepartureAirport = airports.Single(a => a.Code == request.DepartureAirport);
            flight.ArrivalAirport = airports.Single(a => a.Code == request.ArrivalAirport);
            flight.DepartureTime = request.DepartureTime;
            flight.ArrivalTime = request.ArrivalTime;
            flight.Status = request.Status;
            flight.Version = Guid.NewGuid();
        }

        await _db.SaveChangesAsync();

        return new IdCommandResponse() { ID = flight.FlightID };
    }
}