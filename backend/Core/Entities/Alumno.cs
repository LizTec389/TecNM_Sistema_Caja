namespace TecNM.Api.Core.Entities;

public sealed class Alumno
{
    public required string Matricula { get; set; }
    public required string NombreCompleto { get; set; }
    public required string Carrera { get; set; }
    public int Semestre { get; set; }
    public required string Turno { get; set; }
    public required string Estatus { get; set; } // Inscrito, Baja, etc.
    public bool TieneBeca { get; set; }
    public decimal PorcentajeBeca { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

