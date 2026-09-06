# Meteo-openweathermap-discord

Ceci est un bot Discord développé en **C# (.NET 10)** utilisant la bibliothèque **NetCord** et l'API **OpenWeatherMap**. 

Il permet d'obtenir la météo en temps réel, les prévisions sur 7 jours et de configurer des bulletins météo automatiques.

---

## Fonctionnalités

* **`!meteo`** : Affiche la météo actuelle à Nice (température, ressentis, humidité et conditions).
* **`!previsions`** : Donnes les prévisions météo détaillées sur 7 jours (températures min/max et probabilités de pluie).
* **`!bulletin <heure|intervalle>`** : Configure l'envoi automatique du bulletin météo.
  * Exemple de planification quotidienne : `!bulletin 08:30`
  * Exemple de mode test : `!bulletin 30s`
 
---

## Configuration & Installation Local

### 1. Prérequis
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Une clé d'API [OpenWeatherMap](https://openweathermap.org/api)
* Un bot Discord configuré sur le [Discord Developer Portal](https://discord.com/developers/applications) *(avec les Privileged Gateway Intents activés)*.

### 2. Cloner et configurer
1. Clone le dépôt
2. Configurer le appsettings.json avec vos clés.
3. Installer les dépendances présentes dans le .cspro avec "dotnet build" dans le terminal.
4. Lancer l'application "dotnet run" dans le terminal.
