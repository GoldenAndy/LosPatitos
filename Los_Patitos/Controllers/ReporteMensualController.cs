using Microsoft.AspNetCore.Mvc;
using Los_Patitos.Business;
using Los_Patitos.Models;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace Los_Patitos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReporteMensualController : Controller
    {
        private readonly IReporteMensualService _reportes;
        private readonly IBitacoraService _bitacora;

        public ReporteMensualController(
            IReporteMensualService reportes,
            IBitacoraService bitacora)
        {
            _reportes = reportes;
            _bitacora = bitacora;
        }

        public IActionResult Index()
        {
            var lista = _reportes.Listar();
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Generar()
        {
            try
            {
                await _reportes.GenerarReportesMensualesAsync();
                TempData["Ok"] = "Reportes mensuales generados / actualizados correctamente.";
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ReporteMensual",
                    "Error al generar reportes mensuales.",
                    ex);

                TempData["Error"] = "Ocurrió un error al generar los reportes.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LogErrorAsync(string tabla, string descripcion, Exception ex, CancellationToken ct = default)
        {
            await _bitacora.EscribirAsync(new BitacoraEvento
            {
                TablaDeEvento = tabla,
                TipoDeEvento = "Error",
                FechaDeEvento = DateTime.Now,
                DescripcionDeEvento = descripcion,
                StackTrace = ex.ToString(),
                DatosAnteriores = null,
                DatosPosteriores = null
            }, ct);
        }


        [HttpGet]
        public IActionResult Descargar()
        {
            var lista = _reportes.Listar();
            var ahora = DateTime.Now;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Reportes mensuales");

            // 🔤 Fuente general de la hoja
            ws.Cells.Style.Font.Name = "Times New Roman";
            ws.Cells.Style.Font.Size = 11;

            // 🧾 Encabezados (fila 1)
            ws.Cells[1, 1].Value = "Nombre del comercio";
            ws.Cells[1, 2].Value = "Cantidad de cajas";
            ws.Cells[1, 3].Value = "Monto recaudado";
            ws.Cells[1, 4].Value = "Cantidad de SINPEs";
            ws.Cells[1, 5].Value = "Monto total de comisión";
            ws.Cells[1, 6].Value = "Fecha del reporte";
            ws.Cells[1, 7].Value = "Fecha de generación del archivo";

            // Estilo encabezados: negrita
            using (var headerRange = ws.Cells[1, 1, 1, 7])
            {
                headerRange.Style.Font.Bold = true;
            }

            // 📊 Datos (desde la fila 2)
            var fila = 2;
            foreach (var r in lista)
            {
                ws.Cells[fila, 1].Value = r.Comercio?.Nombre;
                ws.Cells[fila, 2].Value = r.CantidadDeCajas;
                ws.Cells[fila, 3].Value = r.MontoTotalRecaudado;
                ws.Cells[fila, 4].Value = r.CantidadDeSINPES;
                ws.Cells[fila, 5].Value = r.MontoTotalComision;
                ws.Cells[fila, 6].Value = r.FechaDelReporte;
                ws.Cells[fila, 7].Value = ahora; // fecha en que se genera el Excel

                // formatos numéricos bonitos
                ws.Cells[fila, 3].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[fila, 5].Style.Numberformat.Format = "#,##0.00";

                // formato fecha/hora
                ws.Cells[fila, 6].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                ws.Cells[fila, 7].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                fila++;
            }

            // 📏 Ajustar ancho de columnas
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            // 📁 Devolver archivo
            var bytes = package.GetAsByteArray();
            var nombreArchivo = $"ReportesMensuales_{ahora:yyyyMMdd_HHmm}.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo
            );
        }
    }
}
