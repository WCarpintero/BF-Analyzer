using System.Text;
using System.Net.Http.Json;
using System.Text.Json;

namespace BFA.Api.Services;

public class AiAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AiAnalysisService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> AnalyzeFeasibilityAsync(string userIdea, string targetName, string offeringType, string location, string industry, string marketData)
    {
        var apiKey = _config["ApiKeys:Gemini"];
        
        // URL para Gemini 2.5 Flash-Lite
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

        // Construimos el cuerpo con los nuevos parámetros
        var requestBody = new
        {
            contents = new[]
            {
                new { 
                    parts = new[] { 
                        new { text = $@"
                            Actúa como un motor de análisis de datos profesional. Analiza la viabilidad de:
                            - Nombre/Producto Específico: {targetName}
                            - Tipo de Oferta: {offeringType}
                            - Idea Detallada: {userIdea}
                            - Ubicación: {location}
                            - Industria: {industry}
                            - Datos de tendencias actuales: {marketData}

                            INSTRUCCIONES DE RESPUESTA (ESTRICTO):
                            1. Puntaje de viabilidad: Un número del 1 al 100 basado en riesgo/oportunidad.
                            2. Análisis: Un resumen profesional de 3 párrafos, con los datos estadísticos más relevantes.
                            3. Tendencia: Una sola palabra (ALTA, MEDIA, BAJA).
                            
                            Formatea tu respuesta de esta manera exacta para que mi sistema pueda procesarla:
                            SCORE: [número]
                            TREND: [palabra]
                            REPORT: 
                            ### 📈 Viabilidad
                            [Tu análisis de viabilidad aquí]

                            ### ⚠️ Retos y Riesgos
                            [Tus retos aquí]

                            ### 💡 Recomendaciones Estratégicas
                            [Tus recomendaciones aquí]"
                        } 
                    } 
                }
            }
        };

        try 
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error en la comunicación con la IA: {error}";
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseBody);
            
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                var text = firstCandidate.GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                return text ?? "La IA no devolvió texto.";
            }

            return "No se encontró un resultado válido en la respuesta de la IA.";
        }
        catch (Exception ex)
        {
            return $"Error interno al procesar el análisis: {ex.Message}";
        }
    }
}