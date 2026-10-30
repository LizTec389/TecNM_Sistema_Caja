namespace TecNM.Api.Core.Entities;

public sealed class AlumnoMatriculaHistorial
{
    public long IdHistorial { get; set; }
    public required string MatriculaAlumno { get; set; }
    public Alumno? Alumno { get; set; }
    public required string MatriculaPrevia { get; set; }
    public int? IdPeriodo { get; set; }
    public PeriodoAcademico? Periodo { get; set; }
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;
    public string? MotivoCambio { get; set; }
}

