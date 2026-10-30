namespace TecNM.Api.Core.Entities;

public sealed class Adeudo
{
    public long IdAdeudo { get; set; }
    public required string Matricula { get; set; }
    public Alumno? Alumno { get; set; }
    public required string ConceptoCobro { get; set; }
    public int IdPeriodo { get; set; }
    public PeriodoAcademico? Periodo { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public required string Estado { get; set; } = "PENDIENTE";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

