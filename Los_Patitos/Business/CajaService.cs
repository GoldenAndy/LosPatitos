using Los_Patitos.Data;
using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;


namespace Los_Patitos.Business
{
    public class CajaService : ICajaService
    {
        private readonly AppDbContext _context;

        public CajaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CajaModel>> GetByComercioAsync(int idComercio)
        {
            return await _context.Caja_G4
                .Where(c => c.IdComercio == idComercio)
                .OrderByDescending(c => c.FechaDeRegistro)
                .ToListAsync();
        }

        public async Task<CajaModel> GetByIdAsync(int id)
        {
            return await _context.Caja_G4.FindAsync(id);
        }

        public async Task<bool> ExistsByNombreAsync(int idComercio, string nombre)
        {
            return await _context.Caja_G4.AnyAsync(c => c.IdComercio == idComercio && c.Nombre == nombre);
        }

        public async Task<bool> ExistsByTelefonoAsync(string telefono)
        {
            return await _context.Caja_G4.AnyAsync(c => c.TelefonoSINPE == telefono && c.Estado);
        }


        public async Task<string> CreateAsync(CajaModel caja)
        {
            try
            {
                if (await ExistsByNombreAsync(caja.IdComercio, caja.Nombre))
                    return "Ya existe una caja con ese nombre para este comercio.";

                if (await ExistsByTelefonoAsync(caja.TelefonoSINPE))
                    return "Ya existe una caja activa con este teléfono SINPE.";

                caja.FechaDeRegistro = DateTime.Now;
                _context.Caja_G4.Add(caja);
                await _context.SaveChangesAsync();

                return null; // null indica éxito
            }
            catch (DbUpdateException ex)
            {
                return "Error de base de datos: " + ex.InnerException?.Message ?? ex.Message;
            }
            catch (Exception ex)
            {
                return "Error inesperado: " + ex.Message;
            }
        }


        public async Task<bool> UpdateAsync(CajaModel caja)
        {
            var cajaDb = await GetByIdAsync(caja.IdCaja);
            if (cajaDb == null) return false;

            cajaDb.Nombre = caja.Nombre;
            cajaDb.Descripcion = caja.Descripcion;
            cajaDb.TelefonoSINPE = caja.TelefonoSINPE;
            cajaDb.Estado = caja.Estado;
            cajaDb.FechaDeModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
