using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface IComercioService
    {
        List<Comercio> Listar();
        Comercio Detalle(int id);
        int Registrar(Comercio nuevo);
        void Actualizar(Comercio cambios);
    }
}