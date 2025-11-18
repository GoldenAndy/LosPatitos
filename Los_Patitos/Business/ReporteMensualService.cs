using Los_Patitos.Models;
using Los_Patitos.Repositories;

namespace Los_Patitos.Business
{
    public class ReporteMensualService : IReporteMensualService
    {
        private readonly IReporteMensualRepository _repo;
        private readonly IComercioRepository _comercios;
        private readonly ICajaRepository _cajas;
        private readonly ISinpeRepository _sinpes;
        private readonly IConfiguracionComercioRepository _configs;

        public ReporteMensualService(
            IReporteMensualRepository repo,
            IComercioRepository comercios,
            ICajaRepository cajas,
            ISinpeRepository sinpes,
            IConfiguracionComercioRepository configs)
        {
            _repo = repo;
            _comercios = comercios;
            _cajas = cajas;
            _sinpes = sinpes;
            _configs = configs;
        }

        public List<ReporteMensual> Listar() => _repo.Listar();

        public async Task GenerarReportesMensualesAsync()
        {
            var hoy = DateTime.Now;
            int year = hoy.Year;
            int month = hoy.Month;

            var comercios = _comercios.Listar(); 

            foreach (var comercio in comercios)
            {
                // 1. CANTIDAD DE CAJAS
                var cajas = await _cajas.ListarPorComercioAsync(comercio.IdComercio);
                int cantidadCajas = cajas.Count;
                var idsCajas = cajas.Select(c => c.IdCaja).ToList();

                // 2 y 3. MontoTotalRecaudado y CantidadDeSINPES (del mes)
                decimal montoTotalRecaudado = 0m;
                int cantidadSinpes = 0;

                if (idsCajas.Any())
                {
                    var sinpes = await _sinpes.ListarPorCajasYMesAsync(idsCajas, year, month);
                    montoTotalRecaudado = sinpes.Sum(s => s.Monto);
                    cantidadSinpes = sinpes.Count;
                }

                // 4. Comisión (usa ConfiguracionComercio)
                var config = _configs.ObtenerPorComercio(comercio.IdComercio);
                decimal porcentaje = 0m;

                if (config != null && config.Comision > 0)
                    porcentaje = config.Comision / 100m; // 20 -> 0.20

                decimal montoComision = montoTotalRecaudado * porcentaje;

                // Buscar si ya hay reporte de este comercio en este mes
                var existente = _repo.ObtenerPorComercioYMes(comercio.IdComercio, year, month);

                if (existente == null)
                {
                    var nuevo = new ReporteMensual
                    {
                        IdComercio = comercio.IdComercio,
                        CantidadDeCajas = cantidadCajas,
                        MontoTotalRecaudado = montoTotalRecaudado,
                        CantidadDeSINPES = cantidadSinpes,
                        MontoTotalComision = montoComision,
                        FechaDelReporte = hoy
                    };

                    _repo.Crear(nuevo);
                }
                else
                {
                    existente.CantidadDeCajas = cantidadCajas;
                    existente.MontoTotalRecaudado = montoTotalRecaudado;
                    existente.CantidadDeSINPES = cantidadSinpes;
                    existente.MontoTotalComision = montoComision;
                    existente.FechaDelReporte = hoy;

                    _repo.Actualizar(existente);
                }
            }
        }
    }
}
