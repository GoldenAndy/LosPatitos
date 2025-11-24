using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Los_Patitos.Data;
using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;

namespace Los_Patitos.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _db;

        public UsuarioRepository(AppDbContext db)
        {
            _db = db;
        }

        //Crear
        public async Task<int> CrearAsync(UsuarioModel nuevo)
        {
            await _db.Usuario_G4.AddAsync(nuevo);
            await _db.SaveChangesAsync();

            return nuevo.IdUsuario;
        }

        //Editar
        public async Task EditarAsync(UsuarioModel usuario)
        {
            _db.Usuario_G4.Update(usuario);
            await _db.SaveChangesAsync();
        }

        //Obtener por id
        public async Task<UsuarioModel?> ObtenerPorIdAsync(int idUsuario)
        {
            return await _db.Usuario_G4.Include(u => u.Comercio).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }


        //Obtener por identificacion unica
        public async Task<UsuarioModel?> ObtenerPorIdentificacionAsync(string identificacion)
        {
            return await _db.Usuario_G4.Include(u => u.Comercio).FirstOrDefaultAsync(u => u.Identificacion == identificacion);
        }

        //Listar por comercio
        public async Task<List<UsuarioModel>> ListarPorComercioAsync(int idComercio)
        {
            return await _db.Usuario_G4.Where(u => u.IdComercio == idComercio).OrderBy(u => u.Nombres).ThenBy(u => u.PrimerApellido).ThenBy(u => u.SegundoApellido).ToListAsync();
        }
    }
}