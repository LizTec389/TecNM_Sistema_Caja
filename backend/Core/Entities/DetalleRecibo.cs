namespace TecNM.Api.Core.Entities;

public sealed class DetalleRecibo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReciboId { get; set; }
    public Recibo? Recibo { get; set; }
    public Guid? ConceptoId { get; set; }
    public required string Descripcion { get; set; }
    public int Cantidad { get; set; }
    public decimal CuotaUnitario { get; set; }
    public decimal DescuentoBeca { get; set; }
    public decimal ImporteTotal { get; set; }
    public string? PeriodoAno { get; set; }
}

