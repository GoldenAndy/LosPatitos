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
        }
    }
}
