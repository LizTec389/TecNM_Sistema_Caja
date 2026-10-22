using Microsoft.EntityFrameworkCore;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data;

public sealed class TecNMDbContext(DbContextOptions<TecNMDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<ConceptoCobro> ConceptosCobro => Set<ConceptoCobro>();
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<DetalleRecibo> DetallesRecibo => Set<DetalleRecibo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Mapeo de Usuarios
        var user = modelBuilder.Entity<User>();
        user.ToTable("users", table =>
            table.HasCheckConstraint("ck_users_role", "role IN ('Admin', 'User')"));
        user.HasKey(item => item.Id).HasName("pk_users");
        user.HasIndex(item => item.Username).IsUnique().HasDatabaseName("uq_users_username");
        user.HasIndex(item => item.Email).IsUnique().HasDatabaseName("uq_users_email");

        user.Property(item => item.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        user.Property(item => item.Username).HasColumnName("username").HasMaxLength(64).IsRequired();
        user.Property(item => item.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        user.Property(item => item.Password).HasColumnName("password").HasMaxLength(512).IsRequired();
        user.Property(item => item.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        user.Property(item => item.Role).HasColumnName("role").HasMaxLength(16).IsRequired();
        user.Property(item => item.IsActive).HasColumnName("is_active").IsRequired();
        user.Property(item => item.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 2. Mapeo de Alumnos
        var alumno = modelBuilder.Entity<Alumno>();
        alumno.ToTable("alumnos");
        alumno.HasKey(a => a.Matricula).HasName("pk_alumnos");
        alumno.Property(a => a.Matricula).HasColumnName("matricula").HasMaxLength(20).IsRequired();
        alumno.Property(a => a.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(160).IsRequired();
        alumno.Property(a => a.Carrera).HasColumnName("carrera").HasMaxLength(100).IsRequired();
        alumno.Property(a => a.Semestre).HasColumnName("semestre").IsRequired();
        alumno.Property(a => a.Turno).HasColumnName("turno").HasMaxLength(20).IsRequired();
        alumno.Property(a => a.Estatus).HasColumnName("estatus").HasMaxLength(30).IsRequired();
        alumno.Property(a => a.TieneBeca).HasColumnName("tiene_beca").IsRequired();
        alumno.Property(a => a.PorcentajeBeca).HasColumnName("porcentaje_beca").HasPrecision(5, 2).IsRequired();
        alumno.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 3. Mapeo de Conceptos de Cobro
        var concepto = modelBuilder.Entity<ConceptoCobro>();
        concepto.ToTable("conceptos_cobro");
        concepto.HasKey(c => c.Id).HasName("pk_conceptos_cobro");
        concepto.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        concepto.Property(c => c.Clave).HasColumnName("clave").HasMaxLength(30).IsRequired();
        concepto.Property(c => c.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        concepto.Property(c => c.CuotaBase).HasColumnName("cuota_base").HasPrecision(12, 4).IsRequired();
        concepto.Property(c => c.AplicaBeca).HasColumnName("aplica_beca").IsRequired();
        concepto.Property(c => c.Categoria).HasColumnName("categoria").HasMaxLength(50).IsRequired();
        concepto.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();

        // 4. Mapeo de Recibos de Caja
        var recibo = modelBuilder.Entity<Recibo>();
        recibo.ToTable("recibos");
        recibo.HasKey(r => r.Id).HasName("pk_recibos");
        recibo.HasIndex(r => r.NumeroRecibo).IsUnique().HasDatabaseName("uq_recibos_numero");

        recibo.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        recibo.Property(r => r.NumeroRecibo).HasColumnName("numero_recibo").ValueGeneratedOnAdd();
        recibo.Property(r => r.FolioAdministrativo).HasColumnName("folio_administrativo").HasMaxLength(50).IsRequired();
        recibo.Property(r => r.FechaOperacion).HasColumnName("fecha_operacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
        recibo.Property(r => r.AlumnoMatricula).HasColumnName("alumno_matricula").HasMaxLength(20).IsRequired();
        recibo.Property(r => r.AlumnoNombre).HasColumnName("alumno_nombre").HasMaxLength(160).IsRequired();
        recibo.Property(r => r.AlumnoCarrera).HasColumnName("alumno_carrera").HasMaxLength(100).IsRequired();
        recibo.Property(r => r.AlumnoSemestre).HasColumnName("alumno_semestre").IsRequired();
        recibo.Property(r => r.AlumnoTurno).HasColumnName("alumno_turno").HasMaxLength(20).IsRequired();
        recibo.Property(r => r.MetodoPago).HasColumnName("metodo_pago").HasMaxLength(30).IsRequired();
        recibo.Property(r => r.ReferenciaBancaria).HasColumnName("referencia_bancaria").HasMaxLength(50);
        recibo.Property(r => r.CuentaBancaria).HasColumnName("cuenta_bancaria").HasMaxLength(50);
        recibo.Property(r => r.FechaBancaria).HasColumnName("fecha_bancaria");
        recibo.Property(r => r.Subtotal).HasColumnName("subtotal").HasPrecision(12, 4).IsRequired();
        recibo.Property(r => r.DescuentoTotal).HasColumnName("descuento_total").HasPrecision(12, 4).IsRequired();
        recibo.Property(r => r.Total).HasColumnName("total").HasPrecision(12, 4).IsRequired();
        recibo.Property(r => r.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        recibo.Property(r => r.MotivoCancelacion).HasColumnName("motivo_cancelacion").HasMaxLength(500);
        recibo.Property(r => r.FechaCancelacion).HasColumnName("fecha_cancelacion");
        recibo.Property(r => r.CajeroUsuario).HasColumnName("cajero_usuario").HasMaxLength(64).IsRequired();
        recibo.Property(r => r.CajeroNombre).HasColumnName("cajero_nombre").HasMaxLength(160).IsRequired();

        recibo.HasMany(r => r.Detalles)
            .WithOne(d => d.Recibo)
            .HasForeignKey(d => d.ReciboId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. Mapeo de Detalles de Recibo
        var detalle = modelBuilder.Entity<DetalleRecibo>();
        detalle.ToTable("detalles_recibo");
        detalle.HasKey(d => d.Id).HasName("pk_detalles_recibo");
        detalle.Property(d => d.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        detalle.Property(d => d.ReciboId).HasColumnName("recibo_id").IsRequired();
        detalle.Property(d => d.ConceptoId).HasColumnName("concepto_id");
        detalle.Property(d => d.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        detalle.Property(d => d.Cantidad).HasColumnName("cantidad").IsRequired();
        detalle.Property(d => d.CuotaUnitario).HasColumnName("cuota_unitario").HasPrecision(12, 4).IsRequired();
        detalle.Property(d => d.DescuentoBeca).HasColumnName("descuento_beca").HasPrecision(12, 4).IsRequired();
        detalle.Property(d => d.ImporteTotal).HasColumnName("importe_total").HasPrecision(12, 4).IsRequired();
        detalle.Property(d => d.PeriodoAno).HasColumnName("periodo_ano").HasMaxLength(20);
    }
}
