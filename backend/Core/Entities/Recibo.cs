namespace TecNM.Api.Core.Entities;

public sealed class Recibo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long NumeroRecibo { get; set; }
    public required string FolioAdministrativo { get; set; }
    public DateTimeOffset FechaOperacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaEfectivaPago { get; set; }
    public required string AlumnoMatricula { get; set; }
    public Alumno? Alumno { get; set; }
    public required string AlumnoNombre { get; set; }
    public required string AlumnoCarrera { get; set; }
    public int AlumnoSemestre { get; set; }
    public required string AlumnoTurno { get; set; }
    public required string MetodoPago { get; set; } // EFECTIVO, DEPOSITO_BANCARIO, TRANSFERENCIA, TARJETA_DEBITO, TARJETA_CREDITO
    public string? ReferenciaBancaria { get; set; }
    public string? CuentaBancaria { get; set; }
    public DateTimeOffset? FechaBancaria { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Total { get; set; }
    public short Status { get; set; } = 1;
    public required string Estado { get; set; } // REGISTRADO, Vigente, PREVIO, CANCELADO, BLOQUEADO
    public string TipoRecibo { get; set; } = "DEFINITIVO"; // PREVIO, DEFINITIVO
    public bool IsLocked { get; set; } = false;
    public long? PolizaId { get; set; }
    public PolizaIngreso? Poliza { get; set; }
    public string? MotivoCancelacion { get; set; }
    public DateTimeOffset? FechaCancelacion { get; set; }
    public required string CajeroUsuario { get; set; }
    public required string CajeroNombre { get; set; }
    public ICollection<DetalleRecibo> Detalles { get; set; } = new List<DetalleRecibo>();
    public ICollection<MovimientoPago> Movimientos { get; set; } = new List<MovimientoPago>();
}
