namespace TecNM.Api.Core.Entities;

public sealed class Recibo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long NumeroRecibo { get; set; }
    public required string FolioAdministrativo { get; set; }
    public DateTimeOffset FechaOperacion { get; set; } = DateTimeOffset.UtcNow;
    public required string AlumnoMatricula { get; set; }
    public required string AlumnoNombre { get; set; }
    public required string AlumnoCarrera { get; set; }
    public int AlumnoSemestre { get; set; }
    public required string AlumnoTurno { get; set; }
    public required string MetodoPago { get; set; } // EFECTIVO, DEPOSITO_BANCARIO
    public string? ReferenciaBancaria { get; set; }
    public string? CuentaBancaria { get; set; }
    public DateTimeOffset? FechaBancaria { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Total { get; set; }
    public required string Estado { get; set; } // REGISTRADO, PREVIO, CANCELADO, BLOQUEADO
    public string? MotivoCancelacion { get; set; }
    public DateTimeOffset? FechaCancelacion { get; set; }
    public required string CajeroUsuario { get; set; }
    public required string CajeroNombre { get; set; }
    public ICollection<DetalleRecibo> Detalles { get; set; } = new List<DetalleRecibo>();
}

