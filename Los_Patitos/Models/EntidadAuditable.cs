
// modelo para los timestamps automáticos de las entidades
namespace Los_Patitos.Models
{
    public abstract class EntidadAuditable
    {
        public DateTime CreatedAtUtc { get; set; } //Fecha creación
        public DateTime? ModifiedAtUtc { get; set; } //Fecha última modificación
    }
}