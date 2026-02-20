using Microsoft.AspNetCore.Mvc;
using BFA.Api.Models;
using BFA.Api.Services;

namespace BFA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeasibilityController : ControllerBase
{
    private readonly MarketDataService _marketService;
    private readonly AiAnalysisService _aiService;

    public FeasibilityController(MarketDataService marketService, AiAnalysisService aiService)
    {
        _marketService = marketService;
        _aiService = aiService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
    {
        try 
        {
            // 1. Obtener datos de tendencias reales usando el Nombre Específico del producto/servicio
            var trends = await _marketService.GetTrendsAsync(request.TargetName, request.TargetLocation);

            // 2. Pasar todos los campos detallados a la IA para un análisis de precisión
            var analysis = await _aiService.AnalyzeFeasibilityAsync(
                request.BusinessIdea,     // La descripción
                request.TargetName,       // El item específico
                request.OfferingType,     // Producto o Servicio
                request.TargetLocation,   // Ciudad/País
                request.Industry,         // Industria
                trends                    // Datos de SerpApi
            );

            // 3. Devolver la respuesta al Frontend
            return Ok(new { result = analysis });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}