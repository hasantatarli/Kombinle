using System.Net.Http.Json;
using Kombinle.Api.Contracts;
using System.Globalization;

namespace Kombinle.Api.Services;

public sealed class WeatherContextService
{
    private readonly HttpClient _httpClient;

    public WeatherContextService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherContextResponseDto> GetContextAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        var url =
            $"https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat}" +
            $"&longitude={lon}" +
            $"&current=temperature_2m,precipitation,snowfall" +
            $"&timezone=auto";

        var data = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(
            url,
            cancellationToken);

        if (data?.Current == null)
            throw new InvalidOperationException("Weather provider returned empty current weather.");

        var temperature = data.Current.Temperature2m;
        var precipitation = data.Current.Precipitation;
        var snowfall = data.Current.Snowfall;

        var weather = MapWeather(temperature, precipitation, snowfall);
        var season = MapSeason(DateTime.UtcNow.Month);

        return new WeatherContextResponseDto(
            Weather: weather,
            Season: season,
            TemperatureC: temperature,
            PrecipitationMm: precipitation,
            SnowfallCm: snowfall,
            Source: "OpenMeteo"
        );
    }

    private static string MapWeather(double temperatureC, double precipitationMm, double snowfallCm)
    {
        if (snowfallCm > 0)
            return "Snow";

        if (precipitationMm > 0.2)
            return "Rain";

        if (temperatureC >= 26)
            return "Hot";

        if (temperatureC <= 7)
            return "Cold";

        return "Clear";
    }

    private static string MapSeason(int month)
    {
        return month switch
        {
            12 or 1 or 2 => "Winter",
            3 or 4 or 5 => "Spring",
            6 or 7 or 8 => "Summer",
            9 or 10 or 11 => "Autumn",
            _ => "Spring"
        };
    }

    private static readonly Dictionary<string, (double Lat, double Lon)> CityMap =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["Istanbul"] = (41.0082, 28.9784),
        ["Ankara"] = (39.9334, 32.8597),
        ["Antalya"] = (36.8969, 30.7133),
        ["Bursa"] = (40.1885, 29.0610),
        ["Konya"] = (37.8746, 32.4932),
        ["Samsun"] = (41.2867, 36.3300)
    };

    public async Task<WeatherContextResponseDto> GetContextByCityAsync(
    string city,
    CancellationToken cancellationToken = default)
    {
        if (!CityMap.TryGetValue(city, out var coords))
            throw new InvalidOperationException($"Unknown city: {city}");

        return await GetContextAsync(
            coords.Lat,
            coords.Lon,
            cancellationToken);
    }

    private sealed class OpenMeteoResponse
    {
        public OpenMeteoCurrent? Current { get; set; }
    }

    private sealed class OpenMeteoCurrent
    {
        [System.Text.Json.Serialization.JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("precipitation")]
        public double Precipitation { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("snowfall")]
        public double Snowfall { get; set; }
    }
}