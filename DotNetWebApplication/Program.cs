using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/weather", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    const string weatherUrl = "https://api.open-meteo.com/v1/forecast?latitude=12.9698&longitude=77.7499&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,wind_speed_10m&timezone=Asia%2FKolkata";

    try
    {
        var client = httpClientFactory.CreateClient();
        using var response = await client.GetAsync(weatherUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var weather = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(cancellationToken);
        return weather is null
            ? Results.Problem("The weather service returned an empty response.")
            : Results.Ok(weather);
    }
    catch (HttpRequestException)
    {
        return Results.Problem("The weather service could not be reached.");
    }
})
.WithName("GetWeather");

app.Run();

public partial class Program;

record OpenMeteoResponse(
    [property: JsonPropertyName("current")] CurrentWeather Current,
    [property: JsonPropertyName("current_units")] CurrentUnits Units);

record CurrentWeather(
    [property: JsonPropertyName("temperature_2m")] double Temperature_2m,
    [property: JsonPropertyName("relative_humidity_2m")] double Relative_Humidity_2m,
    [property: JsonPropertyName("apparent_temperature")] double Apparent_Temperature,
    [property: JsonPropertyName("is_day")] int Is_Day,
    [property: JsonPropertyName("precipitation")] double Precipitation,
    [property: JsonPropertyName("weather_code")] int Weather_Code,
    [property: JsonPropertyName("wind_speed_10m")] double Wind_Speed_10m);

record CurrentUnits(
    [property: JsonPropertyName("temperature_2m")] string Temperature_2m,
    [property: JsonPropertyName("relative_humidity_2m")] string Relative_Humidity_2m,
    [property: JsonPropertyName("apparent_temperature")] string Apparent_Temperature,
    [property: JsonPropertyName("precipitation")] string Precipitation,
    [property: JsonPropertyName("wind_speed_10m")] string Wind_Speed_10m);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
