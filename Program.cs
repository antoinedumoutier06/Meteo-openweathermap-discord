using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using WeatherBot.Services;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 1. Lecture de la section Configuration
string discordToken = builder.Configuration["Configuration:DiscordToken"]?.Trim() 
    ?? throw new InvalidOperationException("DiscordToken introuvable dans la section Configuration de appsettings.json.");

string openWeatherKey = builder.Configuration["Configuration:OpenWeatherKey"]?.Trim() 
    ?? throw new InvalidOperationException("OpenWeatherKey introuvable dans la section Configuration de appsettings.json.");

// 2. Enregistrement des services
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddSingleton(sp => new WeatherService(
    sp.GetRequiredService<HttpClient>(), 
    openWeatherKey
));

builder.Services.AddSingleton<DailyWeatherWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DailyWeatherWorker>());

// 3. Connexion Gateway Discord (NetCord)
builder.Services.AddDiscordGateway(options =>
{
    options.Token = discordToken;
    options.Intents = GatewayIntents.GuildMessages 
                    | GatewayIntents.MessageContent 
                    | GatewayIntents.Guilds;
});

var host = builder.Build();

var client = host.Services.GetRequiredService<GatewayClient>();
var weatherService = host.Services.GetRequiredService<WeatherService>();
var dailyWorker = host.Services.GetRequiredService<DailyWeatherWorker>();

// 4. Gestionnaire d'événements pour les commandes
client.MessageCreate += async message =>
{
    if (message.Author.IsBot) return;

    // Commande : !meteo
    if (message.Content.Equals("!meteo", StringComparison.OrdinalIgnoreCase))
    {
        var weather = await weatherService.GetWeatherWithForecastAsync(43.70, 7.25);

        if (weather?.Current != null)
        {
            var current = weather.Current;
            var desc = current.Weather.FirstOrDefault()?.Description ?? "N/A";

            string response = $"🌤️ **Météo à Nice** :\n" +
                              $"• **Condition :** {desc}\n" +
                              $"• **Température :** {current.Temp}°C (Ressenti {current.FeelsLike}°C)\n" +
                              $"• **Humidité :** {current.Humidity}%";

            await message.ReplyAsync(response);
        }
        else
        {
            await message.ReplyAsync("❌ Impossible de récupérer la météo.");
        }
    }

    // Commande : !previsions
    if (message.Content.Equals("!previsions", StringComparison.OrdinalIgnoreCase))
    {
        var weather = await weatherService.GetWeatherWithForecastAsync(43.70, 7.25);

        if (weather?.Daily != null && weather.Daily.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📅 **Prévisions sur 7 jours - Nice** :\n");

            foreach (var day in weather.Daily.Take(7))
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(day.UnixTimestamp).DateTime;
                var desc = day.Weather.FirstOrDefault()?.Description ?? "N/A";
                int popPercent = (int)(day.PrecipitationProbability * 100);

                sb.AppendLine($"• **{date:ddd dd/MM}** : {desc} | 🌡️ {day.Temp.Min:F0}°C à {day.Temp.Max:F0}°C | 🌧️ Pluie : {popPercent}%");
            }

            await message.ReplyAsync(sb.ToString());
        }
        else
        {
            await message.ReplyAsync("❌ Impossible de récupérer les prévisions météo.");
        }
    }

    // Commande : !bulletin
    if (message.Content.StartsWith("!bulletin", StringComparison.OrdinalIgnoreCase))
    {
        var parts = message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await message.ReplyAsync("ℹ️ **Utilisation :**\n" +
                                     "• `!bulletin 08:30` (Ajuste l'heure quotidienne)\n" +
                                     "• `!bulletin 30s` (Mode test 30 secondes)");
            return;
        }

        string arg = parts[1].ToLower();

        if (arg.EndsWith("s") && int.TryParse(arg.TrimEnd('s'), out int seconds) && seconds > 0)
        {
            dailyWorker.SetIntervalMode(TimeSpan.FromSeconds(seconds));
            await message.ReplyAsync($"⏱️ Bulletin planifié : envoi **toutes les {seconds} secondes**.");
        }
        else if (TimeSpan.TryParse(arg, out TimeSpan time) && time < TimeSpan.FromDays(1))
        {
            dailyWorker.SetScheduledTime(time);
            await message.ReplyAsync($"⏰ Heure du bulletin quotidien réglée sur **{time:hh\\:mm}**.");
        }
        else
        {
            await message.ReplyAsync("❌ Format invalide. Exemples : `!bulletin 08:30` ou `!bulletin 30s`.");
        }
    }
};

await host.RunAsync();
