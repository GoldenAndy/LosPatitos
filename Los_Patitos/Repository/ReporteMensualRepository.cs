using Los_Patitos.Data;
using Los_Patitos.Models;
using Microsoft.EntityFrameworkCore;

namespace Los_Patitos.Repositories
{
    public class ReporteMensualRepository : IReporteMensualRepository
    {
        private readonly AppDbContext _db;

        public ReporteMensualRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<ReporteMensual> Listar()
        {
            return _db.ReportesMensuales
                      .Include(r => r.Comercio)
                      .OrderByDescending(r => r.FechaDelReporte)
                      .ToList();
        }

        public ReporteMensual? ObtenerPorComercioYMes(int idComercio, int year, int month)
        {
            return _db.ReportesMensuales
                .FirstOrDefault(r =>
                    r.IdComercio == idComercio &&
                    r.FechaDelReporte.Year == year &&
                    r.FechaDelReporte.Month == month);
        }

        public int Crear(ReporteMensual nuevo)
        {
            _db.ReportesMensuales.Add(nuevo);
            _db.SaveChanges();
            return nuevo.IdReporte;
        }

        public void Actualizar(ReporteMensual entidad)
        {
            _db.ReportesMensuales.Update(entidad);
            _db.SaveChanges();
        }
    }
}
