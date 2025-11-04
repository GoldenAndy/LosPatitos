using Microsoft.EntityFrameworkCore;
using Los_Patitos.Models;

namespace Los_Patitos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Comercio> Comercios { get; set; }
        public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; }
        public DbSet<TipoComercio> TiposComercio { get; set; }
        public DbSet<CajaModel> Caja_G4 { get; set; }
        public DbSet<SinpeModel> Sinpe_G4 { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

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

            modelBuilder.Entity<SinpeModel>(e =>
            {
                e.ToTable("Sinpe_G4");
                e.HasKey(s => s.IdSinpe);

                e.Property(s => s.TelefonoOrigen)
                    .HasMaxLength(10)
                    .IsRequired();

                e.Property(s => s.NombreOrigen)
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(s => s.TelefonoDestinaria)
                    .HasMaxLength(10)
                    .IsRequired();

                e.Property(s => s.NombreDestinaria)
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(s => s.Monto)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                e.Property(s => s.FechaDeRegistro)
                    .IsRequired();

                e.Property(s => s.Descripcion)
                    .HasMaxLength(50);

                e.Property(s => s.Estado)
                    .IsRequired();

                //FK de Caja
                e.HasOne(s => s.Caja)
                    .WithMany()
                    .HasForeignKey(s => s.IdCaja)
                    .OnDelete(DeleteBehavior.Restrict);
            });



            modelBuilder.Entity<CajaModel>()
                .HasKey(c => c.IdCaja);

            modelBuilder.Entity<CajaModel>()
                .HasOne(c => c.Comercio)
                .WithMany()
                .HasForeignKey(c => c.IdComercio);

            modelBuilder.Entity<CajaModel>()
                .HasIndex(c => new { c.IdComercio, c.Nombre })
                .IsUnique(); //no pueden haber dos cajas con el mismo nombre en el mismo comercio

            modelBuilder.Entity<CajaModel>()
                .HasIndex(c => c.TelefonoSINPE)
                .IsUnique(); //no pueden haber dos cajas activas con el mismo teléfono

            base.OnModelCreating(modelBuilder);
        }
    }
}
