using Los_Patitos.Business;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

            // 2) Configuración del comercio: debe ser Externa (2) o Ambas (3) y estar Activa
            var cfgCom = _config.ObtenerPorComercio(req.IdComercio);
            if (cfgCom is null || !cfgCom.Estado)
                return Unauthorized(new AuthResponse(false, "Configuración inexistente o inactiva.", null, null));

            bool PermiteExterna(int tipo) => tipo == 2 || tipo == 3; // Externa o Ambas
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
            return Ok(new AuthResponse(true, "Token emitido.", token, jwt.ValidTo));
        }


        // 1) Ping público (sin token) para verificar la ruta
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("pong");

        // 2) Ping protegido para probar el Bearer token
        [HttpGet("secure-ping")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SecurePing() => Ok(new { ok = true, msg = "pong seguro" });



    }
}
