using System;

namespace FlightInformationApi;

// --- Standard exceptions ---

/// <summary>General exception with information which can be returned to the user/caller</summary>
public class FlightInformationException : Exception
{
    public string PublicMessage { get; set; }
    public FlightInformationException(string publicMessage)
    {
        PublicMessage = publicMessage;
    }
}

/// <summary>An object needed by the request was not found</summary>
public class NotFoundException : Exception { }

/// <summary>An object being created already exists</summary>
public class AlreadyExistsException : Exception { }

/// <summary>Object has been modified by another party</summary>
public class ModifiedException : Exception { }