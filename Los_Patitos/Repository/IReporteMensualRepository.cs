using Los_Patitos.Models;

namespace Los_Patitos.Repositories
{
    public interface IReporteMensualRepository
    {
        List<ReporteMensual> Listar();
        ReporteMensual? ObtenerPorComercioYMes(int idComercio, int year, int month);
        int Crear(ReporteMensual nuevo);
        void Actualizar(ReporteMensual entidad);
    }
}