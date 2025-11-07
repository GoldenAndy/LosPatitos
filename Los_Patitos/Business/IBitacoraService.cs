using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface IBitacoraService
    {
        Task EscribirAsync(BitacoraEvento evt, CancellationToken ct = default);
        Task EscribirRangoAsync(IEnumerable<BitacoraEvento> eventos, CancellationToken ct = default);
    }
}