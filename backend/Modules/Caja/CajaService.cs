using Microsoft.EntityFrameworkCore;
using TecNM.Api.Core.Entities;
using TecNM.Api.Data;

namespace TecNM.Api.Modules.Caja;

public sealed class CajaService(TecNMDbContext dbContext)
{
    // ==========================================
    // 1. Gestión y Búsqueda de Estudiantes
    // ==========================================
    public async Task<List<AlumnoDTO>> BuscarAlumnosAsync(string? query, CancellationToken cancellationToken)
    {
        var q = query?.Trim().ToLowerInvariant() ?? string.Empty;

        var dbAlumnos = await dbContext.Alumnos
            .Where(a => string.IsNullOrEmpty(q)
                || a.Matricula.ToLower().Contains(q)
                || a.NombreCompleto.ToLower().Contains(q))
            .Take(10)
            .Select(a => ToAlumnoDTO(a))
            .ToListAsync(cancellationToken);

        if (dbAlumnos.Count != 0 || !string.IsNullOrEmpty(q))
        {
            return dbAlumnos;
        }

        // Si la tabla aún no tiene datos de control escolar, retornar catálogo base de prueba
        return GetAlumnosSemilla()
            .Where(a => string.IsNullOrEmpty(q)
                || a.Matricula.ToLower().Contains(q)
                || a.NombreCompleto.ToLower().Contains(q))
            .ToList();
    }

    public async Task<AlumnoDTO?> GetAlumnoAsync(string matricula, CancellationToken cancellationToken)
    {
        var normalizedMatricula = matricula.Trim();
        var alumno = await dbContext.Alumnos
            .SingleOrDefaultAsync(a => a.Matricula == normalizedMatricula, cancellationToken);

        if (alumno is not null)
        {
            return ToAlumnoDTO(alumno);
        }

        return GetAlumnosSemilla()
            .SingleOrDefault(a => a.Matricula.Equals(normalizedMatricula, StringComparison.OrdinalIgnoreCase));
    }

    // ==========================================
    // 2. Catálogo de Conceptos de Cobro
    // ==========================================
    public async Task<List<ConceptoDTO>> GetConceptosAsync(CancellationToken cancellationToken)
    {
        var conceptos = await dbContext.ConceptosCobro
            .Where(c => c.IsActive)
            .OrderBy(c => c.Clave)
            .Select(c => new ConceptoDTO(
                c.Id,
                c.Clave,
                c.Descripcion,
                c.CuotaBase,
                c.AplicaBeca,
                c.Categoria))
            .ToListAsync(cancellationToken);

        if (conceptos.Count != 0)
        {
            return conceptos;
        }

        // Inicializar conceptos base del TecNM si la tabla está vacía
        var semillas = GetConceptosSemilla();
        dbContext.ConceptosCobro.AddRange(semillas);
        await dbContext.SaveChangesAsync(cancellationToken);

        return semillas.Select(c => new ConceptoDTO(
            c.Id,
            c.Clave,
            c.Descripcion,
            c.CuotaBase,
            c.AplicaBeca,
            c.Categoria)).ToList();
    }

    // ==========================================
    // 3. Emisión de Recibos y Cobro
    // ==========================================
    public async Task<ReciboResponse> CrearReciboAsync(
        CrearReciboRequest request,
        string cajeroUsuario,
        string cajeroNombre,
        bool esPrevio,
        CancellationToken cancellationToken)
    {
        var ultimoNumero = await dbContext.Recibos
            .MaxAsync(r => (long?)r.NumeroRecibo, cancellationToken) ?? 128449;

        var nuevoNumero = ultimoNumero + 1;
        var now = DateTimeOffset.UtcNow;
        var folio = $"FOL-{now:yyyy-MM}-{nuevoNumero}";

        var subtotal = request.Detalles.Sum(d => d.CuotaUnitario * d.Cantidad);
        var descuentos = request.Detalles.Sum(d => d.DescuentoBeca * d.Cantidad);
        var total = Math.Max(0, subtotal - descuentos);

        var recibo = new Recibo
        {
            NumeroRecibo = nuevoNumero,
            FolioAdministrativo = folio,
            FechaOperacion = now,
            AlumnoMatricula = request.Matricula.Trim(),
            AlumnoNombre = request.NombreCompleto.Trim(),
            AlumnoCarrera = request.Carrera.Trim(),
            AlumnoSemestre = request.Semestre,
            AlumnoTurno = request.Turno.Trim(),
            MetodoPago = request.MetodoPago.Trim().ToUpperInvariant(),
            ReferenciaBancaria = request.ReferenciaBancaria?.Trim(),
            CuentaBancaria = request.CuentaBancaria?.Trim(),
            FechaBancaria = request.FechaBancaria,
            Subtotal = subtotal,
            DescuentoTotal = descuentos,
            Total = total,
            Estado = esPrevio ? "PREVIO" : "REGISTRADO",
            CajeroUsuario = cajeroUsuario,
            CajeroNombre = cajeroNombre,
            Detalles = request.Detalles.Select(d => new DetalleRecibo
            {
                ConceptoId = d.ConceptoId,
                Descripcion = d.Descripcion.Trim(),
                Cantidad = d.Cantidad,
                CuotaUnitario = d.CuotaUnitario,
                DescuentoBeca = d.DescuentoBeca,
                ImporteTotal = d.ImporteTotal,
                PeriodoAno = d.PeriodoAno
            }).ToList()
        };

        dbContext.Recibos.Add(recibo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToReciboResponse(recibo);
    }

    // ==========================================
    // 4. Cancelación Administrativa de Recibos
    // ==========================================
    public async Task<ReciboResponse?> CancelarReciboAsync(
        Guid reciboId,
        string motivo,
        string usuario,
        CancellationToken cancellationToken)
    {
        var recibo = await dbContext.Recibos
            .Include(r => r.Detalles)
            .SingleOrDefaultAsync(r => r.Id == reciboId, cancellationToken);

        if (recibo is null)
        {
            return null;
        }

        recibo.Estado = "CANCELADO";
        recibo.MotivoCancelacion = motivo.Trim();
        recibo.FechaCancelacion = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToReciboResponse(recibo);
    }

    // ==========================================
    // 5. Consultas y Corte de Caja
    // ==========================================
    public async Task<ReciboResponse?> GetReciboPorNumeroAsync(long numeroRecibo, CancellationToken cancellationToken)
    {
        var recibo = await dbContext.Recibos
            .Include(r => r.Detalles)
            .SingleOrDefaultAsync(r => r.NumeroRecibo == numeroRecibo, cancellationToken);

        return recibo is null ? null : ToReciboResponse(recibo);
    }

    public async Task<List<ReciboResponse>> GetRecibosRecientesAsync(int limite, CancellationToken cancellationToken)
    {
        return await dbContext.Recibos
            .Include(r => r.Detalles)
            .OrderByDescending(r => r.FechaOperacion)
            .Take(limite)
            .Select(r => ToReciboResponse(r))
            .ToListAsync(cancellationToken);
    }

    public async Task<CorteCajaResponse> GetCorteCajaAsync(DateOnly fecha, CancellationToken cancellationToken)
    {
        var inicio = new DateTimeOffset(fecha.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var fin = new DateTimeOffset(fecha.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var recibosDia = await dbContext.Recibos
            .Where(r => r.FechaOperacion >= inicio && r.FechaOperacion <= fin)
            .ToListAsync(cancellationToken);

        var emitidos = recibosDia.Where(r => r.Estado == "REGISTRADO").ToList();
        var cancelados = recibosDia.Count(r => r.Estado == "CANCELADO");

        var totalEfectivo = emitidos
            .Where(r => r.MetodoPago == "EFECTIVO")
            .Sum(r => r.Total);

        var totalBanco = emitidos
            .Where(r => r.MetodoPago == "DEPOSITO_BANCARIO")
            .Sum(r => r.Total);

        return new CorteCajaResponse(
            fecha,
            totalEfectivo,
            totalBanco,
            totalEfectivo + totalBanco,
            emitidos.Count,
            cancelados);
    }

    // ==========================================
    // Mapeos y Datos Semilla
    // ==========================================
    private static AlumnoDTO ToAlumnoDTO(Alumno a) => new(
        a.Matricula,
        a.NombreCompleto,
        a.Carrera,
        a.Semestre,
        a.Turno,
        a.Estatus,
        a.TieneBeca,
        a.PorcentajeBeca);

    private static ReciboResponse ToReciboResponse(Recibo r) => new(
        r.Id,
        r.NumeroRecibo,
        r.FolioAdministrativo,
        r.FechaOperacion,
        r.AlumnoMatricula,
        r.AlumnoNombre,
        r.AlumnoCarrera,
        r.AlumnoSemestre,
        r.AlumnoTurno,
        r.MetodoPago,
        r.ReferenciaBancaria,
        r.CuentaBancaria,
        r.FechaBancaria,
        r.Subtotal,
        r.DescuentoTotal,
        r.Total,
        r.Estado,
        r.MotivoCancelacion,
        r.FechaCancelacion,
        r.CajeroUsuario,
        r.CajeroNombre,
        r.Detalles.Select(d => new DetalleItemResponse(
            d.Id,
            d.ConceptoId,
            d.Descripcion,
            d.Cantidad,
            d.CuotaUnitario,
            d.DescuentoBeca,
            d.ImporteTotal,
            d.PeriodoAno)).ToList());

    private static List<AlumnoDTO> GetAlumnosSemilla() =>
    [
        new("23050243", "ORTEGON HERNANDEZ KLEBER", "Ing. Industrial", 4, "Matutino", "Inscrito", false, 0),
        new("23050118", "GARCIA ZAVALA LUIS FERNANDO", "Ing. Mecatronica", 3, "Matutino", "Inscrito", false, 0),
        new("22050890", "LOPEZ SANCHEZ MARIANA", "Ing. Gestion Empresarial", 6, "Matutino", "Inscrito", true, 50),
        new("24050012", "RODRIGUEZ MORALES CARLOS", "Ing. Informatica", 1, "Vespertino", "Inscrito", false, 0),
        new("21050334", "HERNANDEZ RAMIREZ SOFIA", "Ing. Energias Renovables", 8, "Matutino", "Inscrito", false, 0)
    ];

    private static List<ConceptoCobro> GetConceptosSemilla() =>
    [
        new() { Clave = "KARD", Descripcion = "KARDEX", CuotaBase = 150.0m, AplicaBeca = false, Categoria = "Constancias" },
        new() { Clave = "INSC", Descripcion = "Pago de Inscripción (Periodo Vigente 2026)", CuotaBase = 2600.0m, AplicaBeca = true, Categoria = "Inscripción" },
        new() { Clave = "INSC-ANT", Descripcion = "Adeudo Inscripción (Año 2025)", CuotaBase = 2450.0m, AplicaBeca = false, Categoria = "Inscripción" },
        new() { Clave = "VERA", Descripcion = "Curso de Verano (Por Asignatura)", CuotaBase = 1200.0m, AplicaBeca = false, Categoria = "Cursos" },
        new() { Clave = "CONST", Descripcion = "Constancia de Estudios con Calificaciones", CuotaBase = 100.0m, AplicaBeca = false, Categoria = "Constancias" },
        new() { Clave = "TITU", Descripcion = "Trámite de Titulación Integral y Cédula", CuotaBase = 3800.0m, AplicaBeca = false, Categoria = "Otros" }
    ];
}

