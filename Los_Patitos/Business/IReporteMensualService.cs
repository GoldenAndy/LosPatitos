using Los_Patitos.Models;

namespace Los_Patitos.Business
{
    public interface IReporteMensualService
    {
        List<ReporteMensual> Listar();
        Task GenerarReportesMensualesAsync();
    }
}
