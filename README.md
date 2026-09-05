# Whitefield Weather

A .NET 10 minimal web application that displays current weather for Whitefield, Bengaluru using the Open-Meteo API.

Coordinates: `12.9698, 77.7499`

## Requirements

- .NET SDK 10.0 or later
- Internet access for the Open-Meteo request

## Run the application

From the repository root:

```powershell
dotnet run --project .\DotNetWebApplication\DotNetWebApplication.csproj --urls http://localhost:5189
```

Open http://localhost:5189/ in a browser.

## API

`GET /api/weather`

Returns current temperature, humidity, apparent temperature, precipitation, weather code, and wind speed from Open-Meteo.

## Tests and coverage

```powershell
dotnet test .\DotNetWebApplication.Tests\DotNetWebApplication.Tests.csproj `
  --settings .\DotNetWebApplication.Tests\coverage.runsettings `
  --collect:"XPlat Code Coverage"
```

The tests cover the successful weather response, upstream failures, empty responses, and the HTML weather page.
