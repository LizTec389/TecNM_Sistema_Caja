namespace TecNM.Api.Modules.Caja;

public sealed record AlumnoDTO(
    string Matricula,
    string NombreCompleto,
    string Carrera,
    int Semestre,
    string Turno,
    string Estatus,
    bool TieneBeca,
    decimal PorcentajeBeca);

public sealed record ConceptoDTO(
    Guid Id,
    string Clave,
    string Descripcion,
    decimal CuotaBase,
    bool AplicaBeca,
    string Categoria);

public sealed record DetalleItemRequest(
    Guid? ConceptoId,
    string Descripcion,
    int Cantidad,
    decimal CuotaUnitario,
    decimal DescuentoBeca,
    decimal ImporteTotal,
    string? PeriodoAno);

public sealed record CrearReciboRequest(
    string Matricula,
    string NombreCompleto,
    string Carrera,
    int Semestre,
    string Turno,
    string MetodoPago,
    string? ReferenciaBancaria,
    string? CuentaBancaria,
    DateTimeOffset? FechaBancaria,
    List<DetalleItemRequest> Detalles);

public sealed record DetalleItemResponse(
    Guid Id,
    Guid? ConceptoId,
    string Descripcion,
    int Cantidad,
    decimal CuotaUnitario,
    decimal DescuentoBeca,
    decimal ImporteTotal,
    string? PeriodoAno);

public sealed record ReciboResponse(
    Guid Id,
    long NumeroRecibo,
    string FolioAdministrativo,
    DateTimeOffset FechaOperacion,
    string AlumnoMatricula,
    string AlumnoNombre,
    string AlumnoCarrera,
    int AlumnoSemestre,
    string AlumnoTurno,
    string MetodoPago,
    string? ReferenciaBancaria,
    string? CuentaBancaria,
    DateTimeOffset? FechaBancaria,
    decimal Subtotal,
    decimal DescuentoTotal,
    decimal Total,
    string Estado,
    string? MotivoCancelacion,
    DateTimeOffset? FechaCancelacion,
    string CajeroUsuario,
    string CajeroNombre,
    List<DetalleItemResponse> Detalles);

public sealed record CancelarReciboRequest(
    string Motivo);

public sealed record CorteCajaResponse(
    DateOnly Fecha,
    decimal TotalEfectivo,
    decimal TotalBanco,
    decimal TotalRecaudado,
    int TotalRecibosEmitidos,
    int TotalRecibosCancelados);

