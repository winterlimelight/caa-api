using System.Threading.Tasks;
using FlightInformationApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationApi.Commands;

/// <summary>Delete a flight</summary>
public class DeleteFlightCommandHandler : ICommandHandler<DeleteFlightCommand, EmptyCommandResponse>
{
    private readonly WriteContext _db;
    private readonly IModelValidator _modelValidator;
    
    public DeleteFlightCommandHandler(WriteContext db, IModelValidator modelValidator)
    {
        _db = db;
        _modelValidator = modelValidator;
    }

    async public Task<EmptyCommandResponse> Execute(DeleteFlightCommand request)
    {
        if (!_modelValidator.Validate(request))
            throw new FlightInformationException("Delete flight request must have positive FlightID and non-empty Version");

        var flight = await _db.Flights.FirstOrDefaultAsync(f => f.FlightID == request.FlightID);

        if(flight is null)
            throw new NotFoundException();
        
        if(flight.Version != request.Version)
            throw new ModifiedException();

        _db.Flights.Remove(flight);

        await _db.SaveChangesAsync();

        return new EmptyCommandResponse();
    }
}