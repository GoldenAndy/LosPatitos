using Microsoft.AspNetCore.Identity;

namespace Los_Patitos.Models
{
    public class UsuarioIdentity : IdentityUser
    {
        public int? IdUsuario { get; set; }
    }
}
