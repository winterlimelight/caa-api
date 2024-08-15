using System;
using System.Threading.Tasks;

namespace FlightInformationApi.Commands;

/// <summary>Interface for CommandHandlers</summary>
/// <remarks>All write actions must be done through CommandHandlers</remarks>
/// <typeparam name="TCommand">Type of command to be used at the data related to it</typeparam>
/// <typeparam name="TResponse">Response from the command</typeparam>
public interface ICommandHandler<TCommand, TResponse>
{
	Task<TResponse> Execute(TCommand request);
}

// --- Standard command responses ---

/// <summary>Successful command execution with no return data</summary>
public class EmptyCommandResponse {}

/// <summary>Successful command execution returning an ID</summary>
public class IdCommandResponse
{
    // Normally I'd use GUIDs here as my standard ID type, but an integer has been requested for 
    // the flights so int has been used instead.
	public int ID { get; set; }
}
