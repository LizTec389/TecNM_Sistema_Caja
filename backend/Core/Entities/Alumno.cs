namespace TecNM.Api.Core.Entities;

public sealed class Alumno
{
    public required string Matricula { get; set; }
    public required string NombreCompleto { get; set; }
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? Sexo { get; set; }
    public int? IdCarrera { get; set; }
    public Carrera? CarreraRef { get; set; }
    public required string Carrera { get; set; }
    public int Semestre { get; set; }
    public required string Turno { get; set; }
    public string? NumeroFicha { get; set; }
    public int? CuentaA { get; set; }
    public required string Estatus { get; set; } // Inscrito, No Inscrito, Baja Temporal, Baja Definitiva, Egresado
    public bool TieneBeca { get; set; }
    public decimal PorcentajeBeca { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
