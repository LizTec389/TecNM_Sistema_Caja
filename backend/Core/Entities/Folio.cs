namespace TecNM.Api.Core.Entities;

public sealed class Folio
{
    public long NumeroFolio { get; set; }
    public string Serie { get; set; } = "A";
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    public string? Observaciones { get; set; }
}

