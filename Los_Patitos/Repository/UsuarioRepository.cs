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


        //Buscar usuario de comercio por correo
        public async Task<UsuarioModel?> ObtenerPorCorreoAsync(string correo)
        {
            return await _db.Usuario_G4
                .Include(u => u.Comercio)
                .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);
        }

        //Actualizar IdNetUser
        public async Task ActualizarIdNetUserAsync(int idUsuario, string idNetUser)
        {
            var usuario = await _db.Usuario_G4.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario != null)
            {
                usuario.IdNetUser = idNetUser;
                await _db.SaveChangesAsync();
            }
        }

        //Buscar usuario de negocio por IdNetUser
        public async Task<UsuarioModel?> ObtenerPorIdNetUserAsync(string idNetUser)
        {
            return await _db.Usuario_G4
                .Include(u => u.Comercio)
                .FirstOrDefaultAsync(u => u.IdNetUser == idNetUser);
        }
    }
}