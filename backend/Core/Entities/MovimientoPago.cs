namespace TecNM.Api.Core.Entities;

public sealed class MovimientoPago
{
    public long IdMovimiento { get; set; }
    public Guid ReciboId { get; set; }
    public Recibo? Recibo { get; set; }
    public required string FormaPago { get; set; } // EFECTIVO, DEPOSITO_BANCARIO, TRANSFERENCIA, TARJETA_DEBITO, TARJETA_CREDITO
    public string? ReferenciaBancaria { get; set; }
    public decimal Monto { get; set; }
    public DateTimeOffset FechaMovimiento { get; set; } = DateTimeOffset.UtcNow;
    public required string CajeroUsuario { get; set; }
}

