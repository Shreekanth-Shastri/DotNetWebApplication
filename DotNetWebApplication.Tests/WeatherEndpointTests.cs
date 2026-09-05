using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetWebApplication.Tests;

public sealed class WeatherEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public WeatherEndpointTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task WeatherEndpoint_ReturnsCurrentWeather()
    {
        using var client = CreateClient(HttpStatusCode.OK, "{\"current\":{\"temperature_2m\":28.5,\"relative_humidity_2m\":62,\"apparent_temperature\":29.1,\"is_day\":1,\"precipitation\":0,\"weather_code\":2,\"wind_speed_10m\":8.2},\"current_units\":{\"temperature_2m\":\"°C\",\"relative_humidity_2m\":\"%\",\"apparent_temperature\":\"°C\",\"precipitation\":\"mm\",\"wind_speed_10m\":\"km/h\"}}");

        var response = await client.GetAsync("/api/weather");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"temperature_2m\":28.5", body);
        Assert.Contains("\"weather_code\":2", body);
    }

    [Fact]
    public async Task WeatherEndpoint_ReturnsProblemWhenWeatherServiceFails()
    {
        using var client = CreateClient(HttpStatusCode.BadGateway, "upstream failure");

        var response = await client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("weather service could not be reached", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task WeatherEndpoint_ReturnsProblemWhenWeatherResponseIsEmpty()
    {
        using var client = CreateClient(HttpStatusCode.OK, "null");

        var response = await client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("weather service returned an empty response", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Root_ReturnsWeatherPage()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Whitefield,", body);
        Assert.Contains("12.9698", body);
    }

    private HttpClient CreateClient(HttpStatusCode statusCode, string content)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient(string.Empty).ConfigurePrimaryHttpMessageHandler(() =>
                    new StubHttpMessageHandler(statusCode, content));
            });
        }).CreateClient();
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
        }
    }
}