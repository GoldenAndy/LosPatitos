using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Los_Patitos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Comercio> Comercios { get; set; }
        public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; }
        public DbSet<TipoComercio> TiposComercio { get; set; }
        public DbSet<CajaModel> Caja_G4 { get; set; }
        public DbSet<SinpeModel> Sinpe_G4 { get; set; }
        public DbSet<BitacoraEvento> BITACORA_EVENTOS { get; set; } = null!;

        //AUDITORÍA, helpers JSON y snapshots

        // para guardar objetos en la bitácora en formato JSON sin que deje de funcionar cuando hay relaciones entre tablas
        private static readonly JsonSerializerOptions _auditJson = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // No escribir valores nulos
            ReferenceHandler = ReferenceHandler.IgnoreCycles 
        };

        //para DatosAnteriores y DatosPosteriores, convierte un objeto a JSON o null si es null 
        private static string? ToJson(object? obj) =>
            obj is null ? null : JsonSerializer.Serialize(obj, _auditJson);

        private static string TableName(EntityEntry e) =>
            e.Metadata.GetTableName() ?? e.Metadata.ClrType.Name;

        // Agrega las fechas automáticas de creación y modificación para las entidades auditables antes de guardarlas en la base de datos
        // y asigna CreatedAtUtc o ModifiedAtUtc dependiendo de que accion se haya hecho
        private void TouchAuditFields()
        {
            var now = DateTime.Now;
            foreach (var e in ChangeTracker.Entries<EntidadAuditable>()) 
            {
                if (e.State == EntityState.Added)
                {
                    e.Entity.CreatedAtUtc = now;
                }
                else if (e.State == EntityState.Modified)
                {
                    e.Entity.ModifiedAtUtc = now;
                }
            }
        }

        // Recorre todas las entidades que han cambiado, detecta si fueron Insertadas(Added) o Editadas(Modified)
        // y captura sus valores actuales y anteriores
        private IEnumerable<BitacoraEvento> BuildAuditEntries()
        {
            var now = DateTime.Now;
            var list = new List<BitacoraEvento>();

            foreach (var entry in ChangeTracker.Entries()
                         .Where(x => x.Entity is not BitacoraEvento &&
                                     x.State is EntityState.Added or EntityState.Modified))
            {
                var tabla = TableName(entry);

                Dictionary<string, object?> SnapshotPropsCurrent() //Captura valores actuales de la entidad
                    => entry.Properties
                            .Where(p => !p.Metadata.IsShadowProperty() && !p.IsTemporary)
                            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                Dictionary<string, object?> SnapshotPropsOriginal() //Captura valores originales en el Entity Tracker
                    => entry.Properties
                            .Where(p => !p.Metadata.IsShadowProperty() && !p.IsTemporary)
                            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

                Dictionary<string, object?> SnapshotPropsFromDb() //Intenta obtener los valores reales desde la base de datos
                {
                    try
                    {
                        var dbValues = entry.GetDatabaseValues(); 
                        if (dbValues == null) return SnapshotPropsOriginal();
                        var dict = new Dictionary<string, object?>();
                        foreach (var p in entry.Metadata.GetProperties())
                        {
                            if (p.IsShadowProperty()) continue;
                            dict[p.Name] = dbValues[p];
                        }
                        return dict;
                    }
                    catch
                    {
                        return SnapshotPropsOriginal();
                    }
                }
                if (entry.State == EntityState.Added) // Guarda datos insertados
                {
                    var curr = SnapshotPropsCurrent();
                    list.Add(new BitacoraEvento
                    {
                        TablaDeEvento = tabla,
                        TipoDeEvento = "Registrar",
                        FechaDeEvento = now,
                        DescripcionDeEvento = $"Inserción en {tabla}",
                        StackTrace = string.Empty,
                        DatosAnteriores = ToJson(curr),
                        DatosPosteriores = null
                    });
                }
                else if (entry.State == EntityState.Modified) // Guarda datos editados
                {
                    var antes = SnapshotPropsFromDb();
                    var despues = SnapshotPropsCurrent();

                    list.Add(new BitacoraEvento
                    {
                        TablaDeEvento = tabla,
                        TipoDeEvento = "Editar",
                        FechaDeEvento = now,
                        DescripcionDeEvento = $"Actualización en {tabla}",
                        StackTrace = string.Empty,
                        DatosAnteriores = ToJson(antes),
                        DatosPosteriores = ToJson(despues)
                    });               
                }
            }

            return list;
        }

        // MODELOS y MAPEO 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        //COMERCIO
            modelBuilder.Entity<Comercio>(entity =>
            {
                entity.ToTable("comercio_G4");
                entity.HasKey(e => e.IdComercio);
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

        //SINPE
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

                // FK de Caja
                e.HasOne(s => s.Caja)
                    .WithMany()
                    .HasForeignKey(s => s.IdCaja)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //CAJAS
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

            //BITÁCORA_EVENTOS
            modelBuilder.Entity<BitacoraEvento>(e =>
            {
                e.ToTable("BITACORA_EVENTOS");
                e.HasKey(x => x.IdEvento);

                e.Property(x => x.IdEvento)
                .HasColumnName("idEvento")
                .UseMySqlIdentityColumn();

                e.Property(x => x.TablaDeEvento)
                    .HasColumnName("TablaDeEvento")
                    .HasMaxLength(20).IsRequired();

                e.Property(x => x.TipoDeEvento)
                    .HasColumnName("TipoDeEvento")
                    .HasMaxLength(20).IsRequired();

                e.Property(x => x.FechaDeEvento)
                    .HasColumnName("FechaDeEvento")
                    .IsRequired();

                e.Property(x => x.DescripcionDeEvento)
                    .HasColumnName("DescripcionDeEvento")
                    .HasColumnType("LONGTEXT")
                    .IsRequired();

                e.Property(x => x.StackTrace)
                    .HasColumnName("StackTrace")
                    .HasColumnType("LONGTEXT")
                    .IsRequired();

                e.Property(x => x.DatosAnteriores)
                    .HasColumnName("DatosAnteriores")
                    .HasColumnType("LONGTEXT");

                e.Property(x => x.DatosPosteriores)
                    .HasColumnName("DatosPosteriores")
                    .HasColumnType("LONGTEXT");
            });

            base.OnModelCreating(modelBuilder);
        }

        // SaveChanges con bitácora automática 
        public override int SaveChanges()
        {
            try
            {
                TouchAuditFields(); // Marca CreatedAt/ModifiedAt en entidades auditables
                var audits = BuildAuditEntries().ToList(); 
                var result = base.SaveChanges();

                if (audits.Count > 0) 
                {
                    try
                    {
                        BITACORA_EVENTOS.AddRange(audits);
                        base.SaveChanges();
                    }
                    catch
                    {

                    }
                }

                return result;
            }
            catch (Exception ex)
            {

            // Intento de registrar un evento GLOBAL/Error en bitácora
                try
                {
                    BITACORA_EVENTOS.Add(new BitacoraEvento
                    {
                        TablaDeEvento = "GLOBAL", // para cuando el error no es de una tabla, si no del sistema en general
                        TipoDeEvento = "Error",
                        FechaDeEvento = DateTime.Now,
                        DescripcionDeEvento = ex.Message,
                        StackTrace = ex.StackTrace ?? string.Empty
                    });
                    base.SaveChanges();
                }
                catch {}

                throw; 
            }
        }

        // Sobrescribe SaveChangesAsync para inyectar auditoría sin romper la operación principal
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                TouchAuditFields();
                var audits = BuildAuditEntries().ToList();
                var result = await base.SaveChangesAsync(cancellationToken);

                if (audits.Count > 0)
                {
                    try
                    {
                        await BITACORA_EVENTOS.AddRangeAsync(audits, cancellationToken);
                        await base.SaveChangesAsync(cancellationToken);
                    }
                    catch
                    {
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                try
                {
                    await BITACORA_EVENTOS.AddAsync(new BitacoraEvento
                    {
                        TablaDeEvento = "GLOBAL", 
                        TipoDeEvento = "Error",
                        FechaDeEvento = DateTime.Now,
                        DescripcionDeEvento = ex.Message,
                        StackTrace = ex.StackTrace ?? string.Empty
                    }, cancellationToken);
                    await base.SaveChangesAsync(cancellationToken);
                }
                catch { }

                throw;
            }
        }
    }
}