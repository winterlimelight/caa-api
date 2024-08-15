## To run tests
cd FlightInformationApi.Test
# todo... dotnet test ?...


## To launch swagger
cd FlightInformationApi
dotnet run --launch-profile "swagger"
# then open http://localhost:5083/swagger/


## To directly run the API
cd FlightInformationApi
dotnet run --launch-profile "api"
# then use url like https://localhost:5001/flights/14
