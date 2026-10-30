namespace TecNM.Api.Core.Entities;

public sealed class Carrera
{
    public int IdCarrera { get; set; }
    public required string Abreviatura { get; set; }
    public required string NombreCarrera { get; set; }
    public string? Titulo { get; set; }
    public bool Activo { get; set; } = true;
}

