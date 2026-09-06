using System.Text.Json.Serialization;

namespace WeatherBot.Models;

public record WeatherResponse(
    [property: JsonPropertyName("lat")] double Latitude,
    [property: JsonPropertyName("lon")] double Longitude,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("current")] CurrentWeather Current,
    [property: JsonPropertyName("daily")] List<DailyWeather>? Daily
);

public record CurrentWeather(
    [property: JsonPropertyName("temp")] double Temp,
    [property: JsonPropertyName("feels_like")] double FeelsLike,
    [property: JsonPropertyName("humidity")] int Humidity,
    [property: JsonPropertyName("weather")] List<WeatherDescription> Weather
);

public record DailyWeather(
    [property: JsonPropertyName("dt")] long UnixTimestamp,
    [property: JsonPropertyName("temp")] TempDetails Temp,
    [property: JsonPropertyName("humidity")] int Humidity,
    [property: JsonPropertyName("pop")] double PrecipitationProbability,
    [property: JsonPropertyName("weather")] List<WeatherDescription> Weather
);

public record TempDetails(
    [property: JsonPropertyName("day")] double Day,
    [property: JsonPropertyName("min")] double Min,
    [property: JsonPropertyName("max")] double Max
);

public record WeatherDescription(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("icon")] string Icon
);
