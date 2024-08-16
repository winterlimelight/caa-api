using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using FlightInformationApi.Commands;
using FlightInformationApi.Queries;


namespace FlightInformationApi.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightInformationController : ControllerBase
{
    private readonly ILogger<FlightInformationController> _logger;
    private readonly IFlightQueries _flightQueries;

    public FlightInformationController(ILogger<FlightInformationController> logger, IFlightQueries flightQueries)
    {
        _logger = logger;
        _flightQueries = flightQueries;
    }

    /// <summary>Get all flights</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FlightResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetAll()
    {
        // in a practical app, some kind of paging would be needed for this.
        return Ok(await _flightQueries.GetAllFlights());
    }

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
        _logger.LogTrace("CreateFlight() request=" + JsonSerializer.Serialize(request));

        try
        {
            var result = await handler.Execute(request);
            _logger.LogTrace("CreateFlight() result=" + JsonSerializer.Serialize(result));

            return CreatedAtAction(nameof(GetFlight), new {flightID = result.ID}, result);
        }
        catch(FlightInformationException ex)
        {
            _logger.LogInformation($"CreateFlight() FlightInformationException {ex.PublicMessage}");
            return BadRequest(ex.PublicMessage);
        }
    }

    /// <summary>Update existing flight</summary>
    [HttpPut("{flightID}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IdCommandResponse>> UpdateFlight(int flightID, [FromBody] SetFlightCommand request, [FromServices] ICommandHandler<SetFlightCommand, IdCommandResponse> handler)
    {
        if(flightID <= 0)
            return NotFound();

        request.FlightID = flightID;
        _logger.LogTrace("UpdateFlight() request=" + JsonSerializer.Serialize(request));

        try
        {
            var result = await handler.Execute(request);
            _logger.LogTrace("UpdateFlight() result=" + JsonSerializer.Serialize(result));
            return Ok();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ModifiedException)
        {
            _logger.LogInformation($"UpdateFlight() ModifiedException FlightID={request.FlightID}");
            return Conflict();
        }
        catch(FlightInformationException ex)
        {
            _logger.LogInformation($"UpdateFlight() FlightInformationException {ex.PublicMessage}");
            return BadRequest(ex.PublicMessage);
        }
    }

    // TODO DELETE (id in url + version as query param), 
    // TODO Get /search
}
