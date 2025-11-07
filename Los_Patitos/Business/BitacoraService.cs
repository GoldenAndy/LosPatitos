using Los_Patitos.Data;
using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public class BitacoraService : IBitacoraService
    {
        private readonly IServiceScopeFactory _scopeFactory; //interfaz del sistema de inyección de dependencias para hacer un scope (contenedor DI temporal)

        public BitacoraService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task EscribirAsync(BitacoraEvento evt, CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope(); // Crear un nuevo scope de DI
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // tener un nuevo AppDbContext dentro del scope

            // guardar el evento en la base
            await ctx.BITACORA_EVENTOS.AddAsync(evt, ct);
            await ctx.SaveChangesAsync(ct);
        }

        // para escribir varios eventos agrupados en la bitácora
        public async Task EscribirRangoAsync(IEnumerable<BitacoraEvento> eventos, CancellationToken ct = default)
        {
            var lista = eventos?.ToList();
            if (lista is null || lista.Count == 0) return;

            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // insertar varios registros
            await ctx.BITACORA_EVENTOS.AddRangeAsync(lista, ct);
            await ctx.SaveChangesAsync(ct);
        }
    }
}