using System;

namespace Los_Patitos.API.Dtos
{
    public class SinpeDto
    {
        public int IdSinpe { get; set; }
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string NombreOrigen { get; set; } = string.Empty;
        public string TelefonoDestinatario { get; set; } = string.Empty;
        public string NombreDestinatario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; } //false es No sincronizado y true es Sincronizado
    }
}
