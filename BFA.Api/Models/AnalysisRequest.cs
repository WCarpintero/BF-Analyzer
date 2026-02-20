namespace BFA.Api.Models;

public class AnalysisRequest
{
    public string BusinessIdea { get; set; } = string.Empty;  // La descripción larga
    public string TargetName { get; set; } = string.Empty;    // El producto/servicio específico
    public string OfferingType { get; set; }  = string.Empty;  // "Producto" o "Servicio"
    public string TargetLocation { get; set; }  = string.Empty;
    public string Industry { get; set; } = string.Empty;
}