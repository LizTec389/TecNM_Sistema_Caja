using Microsoft.EntityFrameworkCore;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data;

public sealed class TecNMDbContext(DbContextOptions<TecNMDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<PeriodoAcademico> PeriodosAcademicos => Set<PeriodoAcademico>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<AlumnoMatriculaHistorial> AlumnoMatriculasHistorial => Set<AlumnoMatriculaHistorial>();
    public DbSet<BecaDescuento> BecasDescuentos => Set<BecaDescuento>();
    public DbSet<Adeudo> Adeudos => Set<Adeudo>();
    public DbSet<ConceptoCobro> ConceptosCobro => Set<ConceptoCobro>();
    public DbSet<Folio> Folios => Set<Folio>();
    public DbSet<PolizaIngreso> PolizasIngreso => Set<PolizaIngreso>();
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<DetalleRecibo> DetallesRecibo => Set<DetalleRecibo>();
    public DbSet<MovimientoPago> MovimientosPago => Set<MovimientoPago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Mapeo de Usuarios
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(item => item.Id).HasName("pk_users");
        user.HasIndex(item => item.Username).IsUnique().HasDatabaseName("uq_users_username");
        user.HasIndex(item => item.Email).IsUnique().HasDatabaseName("uq_users_email");

        user.Property(item => item.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        user.Property(item => item.Username).HasColumnName("username").HasMaxLength(64).IsRequired();
        user.Property(item => item.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        user.Property(item => item.Password).HasColumnName("password").HasMaxLength(512).IsRequired();
        user.Property(item => item.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        user.Property(item => item.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
        user.Property(item => item.IsActive).HasColumnName("is_active").IsRequired();
        user.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 2. Mapeo de Carreras
        var carrera = modelBuilder.Entity<Carrera>();
        carrera.ToTable("carreras");
        carrera.HasKey(c => c.IdCarrera).HasName("pk_carreras");
        carrera.Property(c => c.IdCarrera).HasColumnName("id_carrera").ValueGeneratedNever();
        carrera.Property(c => c.Abreviatura).HasColumnName("carrera").HasMaxLength(50).IsRequired();
        carrera.Property(c => c.NombreCarrera).HasColumnName("nombre_carrera").HasMaxLength(150).IsRequired();
        carrera.Property(c => c.Titulo).HasColumnName("titulo").HasMaxLength(150);
        carrera.Property(c => c.Activo).HasColumnName("activo").HasDefaultValue(true);

        // 3. Mapeo de Periodos Académicos
        var periodo = modelBuilder.Entity<PeriodoAcademico>();
        periodo.ToTable("periodos_academicos");
        periodo.HasKey(p => p.IdPeriodo).HasName("pk_periodos_academicos");
        periodo.Property(p => p.IdPeriodo).HasColumnName("id_periodo").ValueGeneratedOnAdd();
        periodo.Property(p => p.Codigo).HasColumnName("codigo").HasMaxLength(30).IsRequired();
        periodo.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(150);
        periodo.Property(p => p.Anio).HasColumnName("anio").IsRequired();
        periodo.Property(p => p.Ciclo).HasColumnName("ciclo").HasMaxLength(20).IsRequired();
        periodo.Property(p => p.FechaInicio).HasColumnName("fecha_inicio");
        periodo.Property(p => p.FechaFin).HasColumnName("fecha_fin");
        periodo.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);

        // 4. Mapeo de Alumnos
        var alumno = modelBuilder.Entity<Alumno>();
        alumno.ToTable("alumnos");
        alumno.HasKey(a => a.Matricula).HasName("pk_alumnos");
        alumno.Property(a => a.Matricula).HasColumnName("matricula").HasMaxLength(20).IsRequired();
        alumno.Property(a => a.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(160).IsRequired();
        alumno.Property(a => a.Nombres).HasColumnName("nombres").HasMaxLength(100);
        alumno.Property(a => a.Apellidos).HasColumnName("apellidos").HasMaxLength(100);
        alumno.Property(a => a.Sexo).HasColumnName("sexo").HasMaxLength(1);
        alumno.Property(a => a.IdCarrera).HasColumnName("id_carrera");
        alumno.Property(a => a.Carrera).HasColumnName("carrera").HasMaxLength(100).IsRequired();
        alumno.Property(a => a.Semestre).HasColumnName("semestre").IsRequired();
        alumno.Property(a => a.Turno).HasColumnName("turno").HasMaxLength(20).IsRequired();
        alumno.Property(a => a.NumeroFicha).HasColumnName("numero_ficha").HasMaxLength(30);
        alumno.Property(a => a.CuentaA).HasColumnName("cuenta_a");
        alumno.Property(a => a.Estatus).HasColumnName("estatus").HasMaxLength(30).IsRequired();
        alumno.Property(a => a.TieneBeca).HasColumnName("tiene_beca").IsRequired();
        alumno.Property(a => a.PorcentajeBeca).HasColumnName("porcentaje_beca").HasPrecision(5, 2).IsRequired();
        alumno.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        alumno.HasOne(a => a.CarreraRef)
            .WithMany()
            .HasForeignKey(a => a.IdCarrera)
            .OnDelete(DeleteBehavior.SetNull);

        // 5. Historial de Matrículas
        var hist = modelBuilder.Entity<AlumnoMatriculaHistorial>();
        hist.ToTable("alumno_matriculas_historial");
        hist.HasKey(h => h.IdHistorial).HasName("pk_alumno_matriculas_historial");
        hist.Property(h => h.IdHistorial).HasColumnName("id_historial").ValueGeneratedOnAdd();
        hist.Property(h => h.MatriculaAlumno).HasColumnName("matricula_alumno").HasMaxLength(20).IsRequired();
        hist.Property(h => h.MatriculaPrevia).HasColumnName("matricula_previa").HasMaxLength(30).IsRequired();
        hist.Property(h => h.IdPeriodo).HasColumnName("id_periodo");
        hist.Property(h => h.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("CURRENT_TIMESTAMP");
        hist.Property(h => h.MotivoCambio).HasColumnName("motivo_cambio");

        hist.HasOne(h => h.Alumno)
            .WithMany()
            .HasForeignKey(h => h.MatriculaAlumno)
            .OnDelete(DeleteBehavior.Cascade);

        hist.HasOne(h => h.Periodo)
            .WithMany()
            .HasForeignKey(h => h.IdPeriodo)
            .OnDelete(DeleteBehavior.SetNull);

        // 6. Becas y Descuentos
        var beca = modelBuilder.Entity<BecaDescuento>();
        beca.ToTable("becas_descuentos");
        beca.HasKey(b => b.IdBeca).HasName("pk_becas_descuentos");
        beca.Property(b => b.IdBeca).HasColumnName("id_beca").ValueGeneratedOnAdd();
        beca.Property(b => b.Matricula).HasColumnName("matricula").HasMaxLength(20).IsRequired();
        beca.Property(b => b.IdPeriodo).HasColumnName("id_periodo").IsRequired();
        beca.Property(b => b.TipoBeca).HasColumnName("tipo_beca").HasMaxLength(50).IsRequired();
        beca.Property(b => b.Porcentaje).HasColumnName("porcentaje").HasPrecision(5, 2);
        beca.Property(b => b.MontoFijo).HasColumnName("monto_fijo").HasPrecision(12, 4);
        beca.Property(b => b.Motivo).HasColumnName("motivo").HasMaxLength(250);
        beca.Property(b => b.Activa).HasColumnName("activa").HasDefaultValue(true);
        beca.Property(b => b.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        beca.HasOne(b => b.Alumno)
            .WithMany()
            .HasForeignKey(b => b.Matricula)
            .OnDelete(DeleteBehavior.Cascade);

        beca.HasOne(b => b.Periodo)
            .WithMany()
            .HasForeignKey(b => b.IdPeriodo)
            .OnDelete(DeleteBehavior.Restrict);

        // 7. Adeudos
        var adeudo = modelBuilder.Entity<Adeudo>();
        adeudo.ToTable("adeudos");
        adeudo.HasKey(a => a.IdAdeudo).HasName("pk_adeudos");
        adeudo.Property(a => a.IdAdeudo).HasColumnName("id_adeudo").ValueGeneratedOnAdd();
        adeudo.Property(a => a.Matricula).HasColumnName("matricula").HasMaxLength(20).IsRequired();
        adeudo.Property(a => a.ConceptoCobro).HasColumnName("concepto_cobro").HasMaxLength(100).IsRequired();
        adeudo.Property(a => a.IdPeriodo).HasColumnName("id_periodo").IsRequired();
        adeudo.Property(a => a.MontoOriginal).HasColumnName("monto_original").HasPrecision(12, 4).IsRequired();
        adeudo.Property(a => a.SaldoPendiente).HasColumnName("saldo_pendiente").HasPrecision(12, 4).IsRequired();
        adeudo.Property(a => a.FechaLimite).HasColumnName("fecha_limite");
        adeudo.Property(a => a.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        adeudo.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        adeudo.HasOne(a => a.Alumno)
            .WithMany()
            .HasForeignKey(a => a.Matricula)
            .OnDelete(DeleteBehavior.Cascade);

        adeudo.HasOne(a => a.Periodo)
            .WithMany()
            .HasForeignKey(a => a.IdPeriodo)
            .OnDelete(DeleteBehavior.Restrict);

        // 8. Conceptos de Cobro
        var concepto = modelBuilder.Entity<ConceptoCobro>();
        concepto.ToTable("conceptos_cobro");
        concepto.HasKey(c => c.Id).HasName("pk_conceptos_cobro");
        concepto.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        concepto.Property(c => c.Clave).HasColumnName("clave").HasMaxLength(30).IsRequired();
        concepto.Property(c => c.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        concepto.Property(c => c.CuotaBase).HasColumnName("cuota_base").HasPrecision(12, 4).IsRequired();
        concepto.Property(c => c.Orden).HasColumnName("orden");
        concepto.Property(c => c.CuentaReferencia).HasColumnName("cuenta_referencia").HasMaxLength(30);
        concepto.Property(c => c.AplicaBeca).HasColumnName("aplica_beca").IsRequired();
        concepto.Property(c => c.Categoria).HasColumnName("categoria").HasMaxLength(50).IsRequired();
        concepto.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();

        // 9. Folios
        var folio = modelBuilder.Entity<Folio>();
        folio.ToTable("folios");
        folio.HasKey(f => f.NumeroFolio).HasName("pk_folios");
        folio.Property(f => f.NumeroFolio).HasColumnName("numero_folio").ValueGeneratedNever();
        folio.Property(f => f.Serie).HasColumnName("serie").HasMaxLength(10).IsRequired();
        folio.Property(f => f.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
        folio.Property(f => f.Observaciones).HasColumnName("observaciones");

        // 10. Pólizas de Ingreso
        var poliza = modelBuilder.Entity<PolizaIngreso>();
        poliza.ToTable("polizas_ingreso");
        poliza.HasKey(p => p.IdPoliza).HasName("pk_polizas_ingreso");
        poliza.Property(p => p.IdPoliza).HasColumnName("id_poliza").ValueGeneratedOnAdd();
        poliza.Property(p => p.NumeroPoliza).HasColumnName("numero_poliza").HasMaxLength(50).IsRequired();
        poliza.Property(p => p.FechaPoliza).HasColumnName("fecha_poliza").IsRequired();
        poliza.Property(p => p.TotalRecaudado).HasColumnName("total_recaudado").HasPrecision(12, 4).IsRequired();
        poliza.Property(p => p.CajeroUsuario).HasColumnName("cajero_usuario").HasMaxLength(64).IsRequired();
        poliza.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        poliza.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 11. Recibos de Caja
        var recibo = modelBuilder.Entity<Recibo>();
        recibo.ToTable("recibos");
        recibo.HasKey(r => r.Id).HasName("pk_recibos");
        recibo.HasIndex(r => r.NumeroRecibo).IsUnique().HasDatabaseName("uq_recibos_numero");

        recibo.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        recibo.Property(r => r.NumeroRecibo).HasColumnName("numero_recibo").ValueGeneratedOnAdd();
        recibo.Property(r => r.FolioAdministrativo).HasColumnName("folio_administrativo").HasMaxLength(50).IsRequired();
        recibo.Property(r => r.FechaOperacion).HasColumnName("fecha_operacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
        recibo.Property(r => r.FechaEfectivaPago).HasColumnName("fecha_efectiva_pago").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
        recibo.Property(r => r.Status).HasColumnName("status").HasDefaultValue((short)1);
        recibo.Property(r => r.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        recibo.Property(r => r.TipoRecibo).HasColumnName("tipo_recibo").HasMaxLength(20).HasDefaultValue("DEFINITIVO");
        recibo.Property(r => r.IsLocked).HasColumnName("is_locked").HasDefaultValue(false);
        recibo.Property(r => r.PolizaId).HasColumnName("poliza_id");
        recibo.Property(r => r.MotivoCancelacion).HasColumnName("motivo_cancelacion").HasMaxLength(500);
        recibo.Property(r => r.FechaCancelacion).HasColumnName("fecha_cancelacion");
        recibo.Property(r => r.CajeroUsuario).HasColumnName("cajero_usuario").HasMaxLength(64).IsRequired();
        recibo.Property(r => r.CajeroNombre).HasColumnName("cajero_nombre").HasMaxLength(160).IsRequired();

        recibo.HasOne(r => r.Alumno)
            .WithMany()
            .HasForeignKey(r => r.AlumnoMatricula)
            .OnDelete(DeleteBehavior.Restrict);

        recibo.HasOne(r => r.Poliza)
            .WithMany()
            .HasForeignKey(r => r.PolizaId)
            .OnDelete(DeleteBehavior.SetNull);

        recibo.HasMany(r => r.Detalles)
            .WithOne(d => d.Recibo)
            .HasForeignKey(d => d.ReciboId)
            .OnDelete(DeleteBehavior.Cascade);

        recibo.HasMany(r => r.Movimientos)
            .WithOne(m => m.Recibo)
            .HasForeignKey(m => m.ReciboId)
            .OnDelete(DeleteBehavior.Cascade);

        // 12. Detalles de Recibo
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

        // 13. Movimientos de Pago
        var mov = modelBuilder.Entity<MovimientoPago>();
        mov.ToTable("movimientos_pago");
        mov.HasKey(m => m.IdMovimiento).HasName("pk_movimientos_pago");
        mov.Property(m => m.IdMovimiento).HasColumnName("id_movimiento").ValueGeneratedOnAdd();
        mov.Property(m => m.ReciboId).HasColumnName("recibo_id").IsRequired();
        mov.Property(m => m.FormaPago).HasColumnName("forma_pago").HasMaxLength(30).IsRequired();
        mov.Property(m => m.ReferenciaBancaria).HasColumnName("referencia_bancaria").HasMaxLength(100);
        mov.Property(m => m.Monto).HasColumnName("monto").HasPrecision(12, 4).IsRequired();
        mov.Property(m => m.FechaMovimiento).HasColumnName("fecha_movimiento").HasDefaultValueSql("CURRENT_TIMESTAMP");
        mov.Property(m => m.CajeroUsuario).HasColumnName("cajero_usuario").HasMaxLength(64).IsRequired();
    }
}
