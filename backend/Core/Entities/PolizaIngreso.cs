namespace TecNM.Api.Core.Entities;

public sealed class PolizaIngreso
{
    public long IdPoliza { get; set; }
    public required string NumeroPoliza { get; set; }
    public DateOnly FechaPoliza { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal TotalRecaudado { get; set; }
    public required string CajeroUsuario { get; set; }
    public string Estado { get; set; } = "ABIERTA";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

