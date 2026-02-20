using System.Net.Http.Json;
using BFA.Api.Models;

namespace BFA.Api.Services;

public class MarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MarketDataService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GetTrendsAsync(string query, string location)
    {
        var apiKey = _config["ApiKeys:SerpApi"];
        
        var safeQuery = Uri.EscapeDataString(query);
        var safeLocation = Uri.EscapeDataString(location ?? "global");

        // 'location' por 'geo' que es el estándar para el engine google_trends
        var url = $"https://serpapi.com/search.json?engine=google_trends&q={safeQuery}&geo={safeLocation}&api_key={apiKey}";

        try 
        {
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
        catch (Exception ex)
        {
            // Arrojar el error detallado a la consola de .NET
            Console.WriteLine($"[MarketDataService Error]: {ex.Message}");
            if (ex.InnerException != null) 
                Console.WriteLine($"[Inner Error]: {ex.InnerException.Message}");

            // Devolvemos un mensaje amigable para que la IA no se confunda
            return "No se pudieron obtener datos de tendencias de Google Trends en este momento.";
        }
    }
}