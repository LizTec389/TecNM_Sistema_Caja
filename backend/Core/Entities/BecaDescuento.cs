namespace TecNM.Api.Core.Entities;

public sealed class BecaDescuento
{
    public long IdBeca { get; set; }
    public required string Matricula { get; set; }
    public Alumno? Alumno { get; set; }
    public int IdPeriodo { get; set; }
    public PeriodoAcademico? Periodo { get; set; }
    public required string TipoBeca { get; set; } = "ACADEMICA";
    public decimal Porcentaje { get; set; }
    public decimal MontoFijo { get; set; }
    public string? Motivo { get; set; }
    public bool Activa { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

