using System;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using FlightInformationApi.Commands;
using FlightInformationApi.Queries;


namespace FlightInformationApi.Controllers;

[ApiController]
[Route("flights")]
public class FlightInformationController : ControllerBase
{
    private readonly ILogger<FlightInformationController> _logger;
    private readonly IFlightQueries _flightQueries;

    public FlightInformationController(ILogger<FlightInformationController> logger, IFlightQueries flightQueries)
    {
        _logger = logger;
        _flightQueries = flightQueries;
    }

    // [HttpGet]
    // public IEnumerable<> GetAll()
    // {
    //     throw new NotImplementedException();
    // }

    /// <summary>Get flight by ID</summary>
    [HttpGet("{flightID}")]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetFlight(int flightID)
    {
        FlightResponse flight = await _flightQueries.GetFlight(flightID);
        return flight != null ? Ok(flight) : NotFound();
    }

    /// <summary>Create new flight</summary>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IdCommandResponse>> CreateFlight([FromBody] SetFlightCommand request, [FromServices] ICommandHandler<SetFlightCommand, IdCommandResponse> handler)
    {
        request.FlightID = 0; // new flight request
        // TODO _logger.LogTrace(); serialized representation of request obj
        try
        {
            var result = await handler.Execute(request);
            return CreatedAtAction(nameof(GetFlight), new {flightID = result.ID}, result);
        }
        catch (AlreadyExistsException)
        {
            // TODO logging
            return BadRequest($"Flight {request.FlightID} already exists");
        }
        catch(FlightInformationException ex)
        {
            // TODO logging
            return BadRequest(ex.PublicMessage);
        }
    }

    /// <summary>Update existing flight</summary>
    [HttpPut("{flightID}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IdCommandResponse>> UpdateFlight(int flightID, [FromBody] SetFlightCommand request, [FromServices] ICommandHandler<SetFlightCommand, IdCommandResponse> handler)
    {
        request.FlightID = flightID;
        // TODO _logger.LogTrace(); serialized representation of request obj
        try
        {
            var result = await handler.Execute(request);
            return Ok();
        }
        catch (NotFoundException)
        {
            // TODO logging
            return NotFound();
        }
        catch (ModifiedException)
        {
            // TODO logging
            return BadRequest($"Flight {request.FlightID} has been modified by another user");
        }
        catch(FlightInformationException ex)
        {
            // TODO logging
            return BadRequest(ex.PublicMessage);
        }
    }

    // TODO DELETE (id in url + version as query param), 
    // TODO Get /search
}
