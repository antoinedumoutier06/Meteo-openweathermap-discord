using System.Net.Http.Json;
using WeatherBot.Models;

namespace WeatherBot.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    // Récupère la météo actuelle + prévisions journalières sur 7 jours
    public async Task<WeatherResponse?> GetWeatherWithForecastAsync(double lat, double lon)
    {
        string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&units=metric&lang=fr&exclude=hourly,minutely&appid={_apiKey}";
        return await _httpClient.GetFromJsonAsync<WeatherResponse>(url);
    }
}
