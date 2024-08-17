The instructions assume that you
- have .NET 8 installed on the machine
- are in a shell (e.g. powershell) which can run `dotnet` commands
- have changed directory to the repository root


### To run tests
1. `cd FlightInformationApi.Test`
2. `dotnet test`


### To directly run the API
The API itself has no UI so another tool like Postman is required to run against it.
1. `cd FlightInformationApi`
2. `dotnet run --launch-profile "api"`
3. Access API endpoints e.g. https://localhost:5001/api/flights


### To launch swagger
Swagger provides an interactive environment for running the API as well as documentation thereof.

1. `cd FlightInformationApi`
2. `dotnet run --launch-profile "swagger"`
3. then open http://localhost:5083/swagger/

Please note only the following airport codes are known (see Program.cs PopulateDatabase()): NZAA, NZCH, NZDN, NZHN, NZOH, NZPM, NZQN, NZWN

Sample POST object:
```
{
  "flightNumber": "ANZ680",
  "airline": "Air New Zealand",
  "departureAirport": "NZWN",
  "arrivalAirport": "NZAA",
  "departureTime": "2024-08-15T20:20:00Z",
  "arrivalTime": "2024-08-15T21:25:00Z",
  "status": "InAir"
}
```