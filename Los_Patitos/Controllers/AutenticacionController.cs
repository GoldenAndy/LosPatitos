using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Los_Patitos.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Los_Patitos.Controllers
{
    [ApiController]
    [Route("api/autenticacion")]
    public class AutenticacionController : ControllerBase
    {
        private readonly IComercioService _comercios;
        private readonly IConfiguracionComercioService _config;
        private readonly IConfiguration _cfg;

        public AutenticacionController(
            IComercioService comercios,
            IConfiguracionComercioService config,
            IConfiguration cfg)
        {
            _comercios = comercios;
            _config = config;
            _cfg = cfg;
        }

        public record AuthRequest(int IdComercio);
        public record AuthResponse(bool EsValido, string? Mensaje, string? Token, DateTime? ExpiraUtc);

        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult EmitirToken([FromBody] AuthRequest req)
        {
            if (req is null || req.IdComercio <= 0)
                return BadRequest(new AuthResponse(false, "IdComercio inválido.", null, null));

            // 1) Comercio existe y está activo
            try
            {
                var comercio = _comercios.Detalle(req.IdComercio);
                if (!comercio.Estado)
                    return Unauthorized(new AuthResponse(false, "Comercio inactivo.", null, null));
            }
            catch
            {
                return Unauthorized(new AuthResponse(false, "Comercio no existe.", null, null));
            }

            // 2) Configuración del comercio: debe ser Externa o Ambas
            var cfgCom = _config.ObtenerPorComercio(req.IdComercio);
            if (cfgCom is null)
                return Unauthorized(new AuthResponse(false, "Sin configuración registrada para el comercio.", null, null));

            // Ajusta estos valores a los que usen ustedes en BD:
            // Ejemplo común: 0=Interna, 1=Externa, 2=Ambas
            bool PermiteExterna(int tipo) => tipo == 1 || tipo == 2;

            if (!PermiteExterna(cfgCom.TipoConfiguracion))
                return Unauthorized(new AuthResponse(false, "Comercio no autorizado para API externa.", null, null));

            // 3) Emitir JWT
            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var keyBytes = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, req.IdComercio.ToString()),
                new Claim("idComercio", req.IdComercio.ToString()),
                new Claim("scope", "sinpe.api")
            };

            var exp = DateTime.UtcNow.AddHours(1);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: exp,
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return Ok(new AuthResponse(true, "OK", token, jwt.ValidTo));
        }
    }
}
