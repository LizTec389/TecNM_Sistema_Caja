namespace TecNM.Api.Core.Entities;

public sealed class ConceptoCobro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Clave { get; set; }
    public required string Descripcion { get; set; }
    public decimal CuotaBase { get; set; }
    public int? Orden { get; set; }
    public string? CuentaReferencia { get; set; }
    public bool AplicaBeca { get; set; }
    public required string Categoria { get; set; }
    public bool IsActive { get; set; } = true;
}
