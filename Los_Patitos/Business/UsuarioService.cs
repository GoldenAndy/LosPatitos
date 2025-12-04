using Los_Patitos.Models;
using Los_Patitos.Repositories;
using System.Text.RegularExpressions;

namespace Los_Patitos.Business
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IComercioRepository _comercioRepo;

        public UsuarioService(IUsuarioRepository usuarioRepo, IComercioRepository comercioRepo)
        {
            _usuarioRepo = usuarioRepo;
            _comercioRepo = comercioRepo;
        }


        // REGISTRAR
        public async Task<(bool ok, string? error, int? idUsuario)> RegistrarAsync(UsuarioModel input)
        {
            // Validaciones para el formulario
            if (string.IsNullOrWhiteSpace(input.Nombres))
                return (false, "El nombre es obligatorio.", null);

            if (string.IsNullOrWhiteSpace(input.PrimerApellido))
                return (false, "El primer apellido es obligatorio.", null);

            if (string.IsNullOrWhiteSpace(input.SegundoApellido))
                return (false, "El segundo apellido es obligatorio.", null);

            if (string.IsNullOrWhiteSpace(input.Identificacion))
                return (false, "La identificación es obligatoria.", null);

            if (!EsIdentificacionValida(input.Identificacion))
                return (false, "La identificación debe tener máximo 10 caracteres.", null);

            if (string.IsNullOrWhiteSpace(input.CorreoElectronico))
                return (false, "El correo electrónico es obligatorio.", null);

            if (!EsCorreoValido(input.CorreoElectronico))
                return (false, "El correo electrónico no es válido.", null);

            // Validaciones del comercio
            var comercio = await _comercioRepo.ObtenerPorIdAsync(input.IdComercio);
            if (comercio is null)
                return (false, "El comercio asociado no existe.", null);

            if (!comercio.Estado)
                return (false, "No se puede registrar usuarios en un comercio inactivo.", null);

            // Validación de identificación única
            var existente = await _usuarioRepo.ObtenerPorIdentificacionAsync(input.Identificacion);
            if (existente is not null)
                return (false, "Ya existe un usuario con esta identificación.", null);

            // Valores automáticos
            input.Estado = true; // usuario activo por defecto
            input.FechaDeRegistro = DateTime.Now;
            input.FechaDeModificacion = null;

            // Registrar
            var id = await _usuarioRepo.CrearAsync(input);

            return (true, null, id);
        }


        // EDITAR
        public async Task<(bool ok, string? error)> EditarAsync(UsuarioModel input)
        {
            var original = await _usuarioRepo.ObtenerPorIdAsync(input.IdUsuario);
            if (original is null)
                return (false, "El usuario no existe.");

            // Validaciones para el formulario
            if (string.IsNullOrWhiteSpace(input.Nombres))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(input.PrimerApellido))
                return (false, "El primer apellido es obligatorio.");

            if (string.IsNullOrWhiteSpace(input.SegundoApellido))
                return (false, "El segundo apellido es obligatorio.");

            if (string.IsNullOrWhiteSpace(input.Identificacion))
                return (false, "La identificación es obligatoria.");

            if (!EsIdentificacionValida(input.Identificacion))
                return (false, "La identificación debe tener máximo 10 caracteres.");

            if (string.IsNullOrWhiteSpace(input.CorreoElectronico))
                return (false, "El correo electrónico es obligatorio.");

            if (!EsCorreoValido(input.CorreoElectronico))
                return (false, "El correo electrónico no es válido.");

            // Validar que la nueva identificación no choque con otro usuario
            var otro = await _usuarioRepo.ObtenerPorIdentificacionAsync(input.Identificacion);
            if (otro is not null && otro.IdUsuario != input.IdUsuario)
                return (false, "Ya existe otro usuario con esta identificación.");

            // Actualizar la info
            original.Nombres = input.Nombres;
            original.PrimerApellido = input.PrimerApellido;
            original.SegundoApellido = input.SegundoApellido;
            original.Identificacion = input.Identificacion;
            original.CorreoElectronico = input.CorreoElectronico;
            original.Estado = input.Estado;
            original.FechaDeModificacion = DateTime.Now;

            // Guardar cambios
            await _usuarioRepo.EditarAsync(original);

            return (true, null);
        }


        // LISTAR POR COMERCIO
        public Task<List<UsuarioModel>> ListarPorComercioAsync(int idComercio)
            => _usuarioRepo.ListarPorComercioAsync(idComercio);


        // OBTENER POR ID (para GET Editar)
        public async Task<UsuarioModel?> ObtenerPorIdAsync(int idUsuario)
        {
            return await _usuarioRepo.ObtenerPorIdAsync(idUsuario);
        }



        // Validaciones con expresiones regulares
        private static bool EsIdentificacionValida(string id)
            => Regex.IsMatch(id, @"^\d{1,10}$");

        private static bool EsCorreoValido(string email)
            => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public Task<UsuarioModel?> ObtenerPorCorreoAsync(string correo)
            => _usuarioRepo.ObtenerPorCorreoAsync(correo);

        public Task ActualizarIdNetUserAsync(int idUsuario, string idNetUser)
            => _usuarioRepo.ActualizarIdNetUserAsync(idUsuario, idNetUser);

        public Task<UsuarioModel?> ObtenerPorIdNetUserAsync(string idNetUser)
            => _usuarioRepo.ObtenerPorIdNetUserAsync(idNetUser);

    
    }

}