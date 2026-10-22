using Microsoft.AspNetCore.Mvc;

namespace TecNM.Api.Modules.Caja;

[ApiController]
[Route("api/caja")]
public sealed class CajaController(CajaService cajaService) : ControllerBase
{
    // =========================================================================
    // 1. Alumnos / Control Escolar
    // =========================================================================

    [HttpGet("alumnos/buscar")]
    public async Task<ActionResult<List<AlumnoDTO>>> BuscarAlumnos(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var resultados = await cajaService.BuscarAlumnosAsync(q, cancellationToken);
        return Ok(resultados);
    }

    [HttpGet("alumnos/{matricula}")]
    public async Task<ActionResult<AlumnoDTO>> GetAlumno(
        [FromRoute] string matricula,
        CancellationToken cancellationToken)
    {
        var alumno = await cajaService.GetAlumnoAsync(matricula, cancellationToken);
        if (alumno is null)
        {
            return NotFound(new { message = $"No se encontró al alumno con matrícula '{matricula}'." });
        }

        return Ok(alumno);
    }

    // =========================================================================
    // 2. Catálogo de Conceptos de Cobro
    // =========================================================================

    [HttpGet("conceptos")]
    public async Task<ActionResult<List<ConceptoDTO>>> GetConceptos(CancellationToken cancellationToken)
    {
        var conceptos = await cajaService.GetConceptosAsync(cancellationToken);
        return Ok(conceptos);
    }

    // =========================================================================
    // 3. Emisión de Recibos de Caja
    // =========================================================================

    [HttpPost("recibos")]
    public async Task<ActionResult<ReciboResponse>> CrearRecibo(
        [FromBody] CrearReciboRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.Detalles == null || request.Detalles.Count == 0)
        {
            return BadRequest(new { message = "El recibo debe contener al menos un concepto de cobro." });
        }

        if (string.IsNullOrWhiteSpace(request.Matricula) || string.IsNullOrWhiteSpace(request.NombreCompleto))
        {
            return BadRequest(new { message = "La matrícula y el nombre del estudiante son obligatorios." });
        }

        var (cajeroUsuario, cajeroNombre) = ObtenerDatosCajero();

        var recibo = await cajaService.CrearReciboAsync(
            request,
            cajeroUsuario,
            cajeroNombre,
            esPrevio: false,
            cancellationToken);

        return CreatedAtAction(nameof(GetReciboPorNumero), new { numero = recibo.NumeroRecibo }, recibo);
    }

    [HttpPost("recibos/previo")]
    public async Task<ActionResult<ReciboResponse>> CrearReciboPrevio(
        [FromBody] CrearReciboRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.Detalles == null || request.Detalles.Count == 0)
        {
            return BadRequest(new { message = "El recibo provisional debe contener al menos un concepto." });
        }

        var (cajeroUsuario, cajeroNombre) = ObtenerDatosCajero();

        var recibo = await cajaService.CrearReciboAsync(
            request,
            cajeroUsuario,
            cajeroNombre,
            esPrevio: true,
            cancellationToken);

        return Ok(recibo);
    }

    // =========================================================================
    // 4. Cancelación de Recibos
    // =========================================================================

    [HttpPost("recibos/{id:guid}/cancelar")]
    public async Task<ActionResult<ReciboResponse>> CancelarRecibo(
        [FromRoute] Guid id,
        [FromBody] CancelarReciboRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 5)
        {
            return BadRequest(new { message = "Debe proporcionar un motivo formal de cancelación de al menos 5 caracteres." });
        }

        var (cajeroUsuario, _) = ObtenerDatosCajero();

        var cancelado = await cajaService.CancelarReciboAsync(id, request.Motivo, cajeroUsuario, cancellationToken);
        if (cancelado is null)
        {
            return NotFound(new { message = $"No se encontró el recibo con ID '{id}'." });
        }

        return Ok(cancelado);
    }

    // =========================================================================
    // 5. Consultas y Corte Diario
    // =========================================================================

    [HttpGet("recibos/{numero:long}")]
    public async Task<ActionResult<ReciboResponse>> GetReciboPorNumero(
        [FromRoute] long numero,
        CancellationToken cancellationToken)
    {
        var recibo = await cajaService.GetReciboPorNumeroAsync(numero, cancellationToken);
        if (recibo is null)
        {
            return NotFound(new { message = $"No se encontró el recibo número {numero}." });
        }

        return Ok(recibo);
    }

    [HttpGet("recibos/recientes")]
    public async Task<ActionResult<List<ReciboResponse>>> GetRecibosRecientes(
        [FromQuery] int limite = 15,
        CancellationToken cancellationToken = default)
    {
        var cantidad = Math.Clamp(limite, 1, 50);
        var recibos = await cajaService.GetRecibosRecientesAsync(cantidad, cancellationToken);
        return Ok(recibos);
    }

    [HttpGet("corte")]
    public async Task<ActionResult<CorteCajaResponse>> GetCorteCaja(
        [FromQuery] DateOnly? fecha,
        CancellationToken cancellationToken)
    {
        var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var corte = await cajaService.GetCorteCajaAsync(fechaConsulta, cancellationToken);
        return Ok(corte);
    }

    // =========================================================================
    // Helpers
    // =========================================================================
    private (string Usuario, string Nombre) ObtenerDatosCajero()
    {
        var usuario = User.FindFirst("name")?.Value
            ?? User.Identity?.Name
            ?? "cajero_general";

        var nombre = User.FindFirst("fullName")?.Value
            ?? "Cajero en Turno";

        return (usuario, nombre);
    }
}

