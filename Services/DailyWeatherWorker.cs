using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Rest;

namespace WeatherBot.Services;

public class DailyWeatherWorker : BackgroundService
{
    private readonly GatewayClient _client;
    private readonly WeatherService _weatherService;
    private readonly ILogger<DailyWeatherWorker> _logger;
    private readonly ulong _targetChannelId;

    private bool _isIntervalMode = false;
    private TimeSpan _interval = TimeSpan.FromSeconds(30);
    private TimeSpan _scheduledTime = new TimeSpan(8, 0, 0);

    private CancellationTokenSource? _delayCts;

    public DailyWeatherWorker(
        GatewayClient client, 
        WeatherService weatherService, 
        ILogger<DailyWeatherWorker> logger,
        IConfiguration config)
    {
        _client = client;
        _weatherService = weatherService;
        _logger = logger;

        // Lecture sous le nœud "Configuration"
        _targetChannelId = config.GetValue<ulong>("Configuration:TargetChannelId");
    }

    public void SetScheduledTime(TimeSpan time)
    {
        _scheduledTime = time;
        _isIntervalMode = false;
        _logger.LogInformation("Nouvelle heure du bulletin configurée : {Time}", time);
        _delayCts?.Cancel();
    }

    public void SetIntervalMode(TimeSpan interval)
    {
        _interval = interval;
        _isIntervalMode = true;
        _logger.LogInformation("Mode intervalle activé : toutes les {Seconds}s", interval.TotalSeconds);
        _delayCts?.Cancel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;

            if (_isIntervalMode)
            {
                delay = _interval;
                _logger.LogInformation("Prochain bulletin dans {Seconds} secondes.", delay.TotalSeconds);
            }
            else
            {
                var now = DateTime.Now;
                var nextRun = now.Date.Add(_scheduledTime);

                if (now >= nextRun)
                {
                    nextRun = nextRun.AddDays(1);
                }

                delay = nextRun - now;
                _logger.LogInformation("Prochain bulletin prévu à {NextRun} (dans {Hours}h {Minutes}m)", 
                    nextRun, (int)delay.TotalHours, delay.Minutes);
            }

            _delayCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            try
            {
                await Task.Delay(delay, _delayCts.Token);
            }
            catch (TaskCanceledException)
            {
                continue;
            }

            try
            {
                var weather = await _weatherService.GetWeatherWithForecastAsync(43.70, 7.25);
                if (weather?.Current != null)
                {
                    var current = weather.Current;
                    var desc = current.Weather.FirstOrDefault()?.Description ?? "N/A";

                    string message = $"🌅 **Bulletin Météo - Nice**\n" +
                                     $"• **Condition :** {desc}\n" +
                                     $"• **Température actuelle :** {current.Temp}°C (Ressenti {current.FeelsLike}°C)\n" +
                                     $"• **Humidité :** {current.Humidity}%\n\n" +
                                     $"*Pour voir les prévisions sur 7 jours, tapez `!previsions`.*";

                    await _client.Rest.SendMessageAsync(_targetChannelId, message, cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi du bulletin météo.");
            }
        }
    }
}
