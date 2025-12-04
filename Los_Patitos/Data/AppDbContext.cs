using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Los_Patitos.Data
{
    public class AppDbContext : IdentityDbContext<UsuarioIdentity, RolIdentity, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets 
        public DbSet<Comercio> Comercios { get; set; }
        public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; }
        public DbSet<TipoComercio> TiposComercio { get; set; }
        public DbSet<CajaModel> Caja_G4 { get; set; }
        public DbSet<SinpeModel> Sinpe_G4 { get; set; }
        public DbSet<BitacoraEvento> BITACORA_EVENTOS { get; set; } = null!;
        public DbSet<ConfiguracionComercio> ConfiguracionesComercio { get; set; }
        public DbSet<ReporteMensual> ReportesMensuales { get; set; }
        public DbSet<UsuarioModel> Usuario_G4 { get; set; }


        // AUDITORIA JSON 
        // opciones para serializar objetos a JSON en la auditoría
        private static readonly JsonSerializerOptions _auditJson = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // No escribir nulls
            ReferenceHandler = ReferenceHandler.IgnoreCycles // para que no hayan ciclos en navegación
        };

        // Convierte objeto a JSON (o null si no existe)
        private static string? ToJson(object? obj) =>
            obj is null ? null : JsonSerializer.Serialize(obj, _auditJson);

        // llama al nombre de la tabla asociada a la entidad
        private static string TableName(EntityEntry e) =>
            e.Metadata.GetTableName() ?? e.Metadata.ClrType.Name;

        // Asigna timestamps de creación/modificación a auditoria
        private void TouchAuditFields()
        {
            var now = DateTime.Now;
            foreach (var e in base.ChangeTracker.Entries<EntidadAuditable>())
            {
                if (e.State == EntityState.Added)
                {
                    e.Entity.CreatedAtUtc = now; // Fecha de creación
                }
                else if (e.State == EntityState.Modified)
                {
                    e.Entity.ModifiedAtUtc = now; // Fecha de modificación
                }
            }
        }

        // Construye registros de auditoría para operaciones Insert/Edit
        private IEnumerable<BitacoraEvento> BuildAuditEntries()
        {
            var now = DateTime.Now;
            var list = new List<BitacoraEvento>();

            foreach (var entry in base.ChangeTracker.Entries()
                     .Where(x => x.Entity is not BitacoraEvento &&
                                 x.State is EntityState.Added or EntityState.Modified))
            {
                var tabla = TableName(entry);

                // Valores actuales de la entidad
                Dictionary<string, object?> SnapshotPropsCurrent() // aca Dictionary es para representar los nombres de las propiedades de la entidad y sus valores
                    => entry.Properties
                           .Where(p => !p.Metadata.IsShadowProperty() && !p.IsTemporary)
                           .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                // Valores originales en el ChangeTracker
                Dictionary<string, object?> SnapshotPropsOriginal()
                    => entry.Properties
                           .Where(p => !p.Metadata.IsShadowProperty() && !p.IsTemporary)
                           .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

                // Intenta obtener valores reales desde la BD
                Dictionary<string, object?> SnapshotPropsFromDb()
                {
                    try
                    {
                        var values = entry.GetDatabaseValues();
                        if (values == null) return SnapshotPropsOriginal();

                        return entry.Metadata.GetProperties()
                            .Where(p => !p.IsShadowProperty())
                            .ToDictionary(p => p.Name, p => values[p]);
                    }
                    catch
                    {
                        return SnapshotPropsOriginal(); // Fallback
                    }
                }

                // Auditoría de inserción
                if (entry.State == EntityState.Added)
                {
                    var curr = SnapshotPropsCurrent();
                    list.Add(new BitacoraEvento
                    {
                        TablaDeEvento = tabla,
                        TipoDeEvento = "Registrar",
                        FechaDeEvento = now,
                        DescripcionDeEvento = $"Inserción en {tabla}",
                        StackTrace = string.Empty, // Sin error
                        DatosAnteriores = ToJson(curr), // Datos insertados
                        DatosPosteriores = null
                    });
                }
                // Auditoría de actualización
                else if (entry.State == EntityState.Modified)
                {
                    var antes = SnapshotPropsFromDb();
                    var despues = SnapshotPropsCurrent();

                    list.Add(new BitacoraEvento
                    {
                        TablaDeEvento = tabla,
                        TipoDeEvento = "Editar",
                        FechaDeEvento = now,
                        DescripcionDeEvento = $"Actualización en {tabla}",
                        StackTrace = string.Empty, // Sin error
                        DatosAnteriores = ToJson(antes), // Valores previos
                        DatosPosteriores = ToJson(despues) // Nuevos valores
                    });
                }
            }

            return list;
        }

        // MODELOS y MAPEO 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // COMERCIO
            modelBuilder.Entity<Comercio>(e =>
            {
                e.ToTable("comercio_G4");
                e.HasKey(x => x.IdComercio);
            });

            modelBuilder.Entity<TipoIdentificacion>(e =>
            {
                e.ToTable("TipoIdentificacion_G4");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nombre).HasMaxLength(20);
            });

            modelBuilder.Entity<TipoComercio>(e =>
            {
                e.ToTable("TipoComercio_G4");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nombre).HasMaxLength(50);
            });

            // SINPE
            modelBuilder.Entity<SinpeModel>(e =>
            {
                e.ToTable("Sinpe_G4");
                e.HasKey(s => s.IdSinpe);

                e.Property(s => s.TelefonoOrigen).HasMaxLength(10).IsRequired();
                e.Property(s => s.NombreOrigen).HasMaxLength(200).IsRequired();
                e.Property(s => s.TelefonoDestinaria).HasMaxLength(10).IsRequired();
                e.Property(s => s.NombreDestinaria).HasMaxLength(200).IsRequired();
                e.Property(s => s.Monto).HasColumnType("decimal(18,2)").IsRequired();
                e.Property(s => s.FechaDeRegistro).IsRequired();
                e.Property(s => s.Descripcion).HasMaxLength(50);
                e.Property(s => s.Estado).IsRequired();

                e.HasOne(s => s.Caja)
                    .WithMany()
                    .HasForeignKey(s => s.IdCaja)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Caja
            modelBuilder.Entity<CajaModel>()
                .HasKey(c => c.IdCaja);

            modelBuilder.Entity<CajaModel>()
                .HasOne(c => c.Comercio)
                .WithMany()
                .HasForeignKey(c => c.IdComercio);

            modelBuilder.Entity<CajaModel>()
                .HasIndex(c => new { c.IdComercio, c.Nombre })
                .IsUnique();

            modelBuilder.Entity<CajaModel>()
                .HasIndex(c => c.TelefonoSINPE)
                .IsUnique();


            // CONFIGURACIÓN DE COMERCIO
            modelBuilder.Entity<ConfiguracionComercio>(e =>
            {
                e.ToTable("ConfiguracionComercio_G4");

                e.HasKey(x => x.IdConfiguracion);

                e.Property(x => x.TipoConfiguracion).IsRequired();
                e.Property(x => x.Comision).IsRequired();
                e.Property(x => x.FechaDeRegistro).IsRequired();
                e.Property(x => x.Estado).IsRequired();
                e.HasOne(x => x.Comercio)
                 .WithMany()
                 .HasForeignKey(x => x.IdComercio)
                 .OnDelete(DeleteBehavior.Restrict);


                e.HasIndex(x => x.IdComercio)
                 .IsUnique();
            });

            // REPORTE MENSUAL
            modelBuilder.Entity<ReporteMensual>(e =>
            {
                e.ToTable("ReporteMensual_G4");

                e.HasKey(x => x.IdReporte);

                e.Property(x => x.CantidadDeCajas).IsRequired();
                e.Property(x => x.MontoTotalRecaudado)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                e.Property(x => x.CantidadDeSINPES).IsRequired();
                e.Property(x => x.MontoTotalComision)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                e.Property(x => x.FechaDelReporte).IsRequired();

                e.HasOne(x => x.Comercio)
                    .WithMany()
                    .HasForeignKey(x => x.IdComercio)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BITACORA EVENTOS
            modelBuilder.Entity<BitacoraEvento>(e =>
            {
                e.ToTable("BITACORA_EVENTOS");
                e.HasKey(x => x.IdEvento);

                e.Property(x => x.IdEvento)
                    .HasColumnName("idEvento")
                    .UseMySqlIdentityColumn();

                e.Property(x => x.TablaDeEvento).HasMaxLength(20).IsRequired();
                e.Property(x => x.TipoDeEvento).HasMaxLength(20).IsRequired();
                e.Property(x => x.FechaDeEvento).IsRequired();
                e.Property(x => x.DescripcionDeEvento).HasColumnType("LONGTEXT").IsRequired();
                e.Property(x => x.StackTrace).HasColumnType("LONGTEXT").IsRequired();
                e.Property(x => x.DatosAnteriores).HasColumnType("LONGTEXT");
                e.Property(x => x.DatosPosteriores).HasColumnType("LONGTEXT");
            });

            //Usuario
            modelBuilder.Entity<UsuarioModel>(e =>
            {
                e.ToTable("Usuario_G4");            

                e.HasKey(x => x.IdUsuario);

                e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();

                e.Property(x => x.PrimerApellido).HasMaxLength(100).IsRequired();

                e.Property(x => x.SegundoApellido).HasMaxLength(100).IsRequired();

                e.Property(x => x.Identificacion).HasMaxLength(10).IsRequired();

                e.Property(x => x.CorreoElectronico).HasMaxLength(200).IsRequired();

                e.Property(x => x.FechaDeRegistro).IsRequired();

                e.Property(x => x.FechaDeModificacion).IsRequired(false);  

                e.Property(x => x.Estado).IsRequired();
                
                e.Property(x => x.IdNetUser).HasMaxLength(36).IsRequired(false);

                //FK con Comercio
                e.HasOne(x => x.Comercio).WithMany().HasForeignKey(x => x.IdComercio).OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.Identificacion).IsUnique();
            });

        }

        // SaveChanges para auditoría automática
        public override int SaveChanges()
        {
            TouchAuditFields(); // para fechas de creación/modificación
            var audits = BuildAuditEntries().ToList();

            var result = base.SaveChanges(); //guardar

            // Si hubo auditoría, la guarda después sin romper la funcionalidad de todo
            if (audits.Count > 0)
            {
                try
                {
                    BITACORA_EVENTOS.AddRange(audits); // Agrega eventos a la bitácora
                    base.SaveChanges();
                }
                catch { }
            }

            return result;
        }

        // SaveChanges pero asíncrona
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TouchAuditFields();
            var audits = BuildAuditEntries().ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (audits.Count > 0)
            {
                try
                {
                    await BITACORA_EVENTOS.AddRangeAsync(audits, cancellationToken); // Agrega bitácora
                    await base.SaveChangesAsync(cancellationToken); // guarda y deja cancelar la operación async si el request se interrumpe
                }
                catch { }
            }

            return result;
        }
    }
}
