namespace TecNM.Api.Core.Entities;

public sealed class PeriodoAcademico
{
    public int IdPeriodo { get; set; }
    public required string Codigo { get; set; }
    public string? Descripcion { get; set; }
    public short Anio { get; set; }
    public required string Ciclo { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public bool Activo { get; set; } = true;
}

