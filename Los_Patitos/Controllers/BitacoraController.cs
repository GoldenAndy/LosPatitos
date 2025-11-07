using Los_Patitos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Los_Patitos.Controllers
{
    public class BitacoraController : Controller
    {
        private readonly AppDbContext _db;
        public BitacoraController(AppDbContext db) => _db = db;

        // Lista para bitácora con filtros por tabla, tipo, texto y FECHA (día exacto)
        public async Task<IActionResult> Index(string? tabla, string? tipo, string? q, DateTime? fecha, int take = 500, CancellationToken ct = default)
        {
            // query sin tracking
            var qry = _db.BITACORA_EVENTOS.AsNoTracking();

            // Filtros 
            if (!string.IsNullOrWhiteSpace(tabla))
                qry = qry.Where(b => b.TablaDeEvento == tabla);

            if (!string.IsNullOrWhiteSpace(tipo))
                qry = qry.Where(b => b.TipoDeEvento == tipo);

            if (!string.IsNullOrWhiteSpace(q))
            {
                qry = qry.Where(b =>
                    b.DescripcionDeEvento.Contains(q) ||
                    (b.DatosAnteriores != null && b.DatosAnteriores.Contains(q)) ||
                    (b.DatosPosteriores != null && b.DatosPosteriores.Contains(q)));
            }

            // Filtro por fecha de evento
            if (fecha.HasValue)
            {
                var dayStart = fecha.Value.Date;          
                var dayEnd = dayStart.AddDays(1);      
                qry = qry.Where(b => b.FechaDeEvento >= dayStart && b.FechaDeEvento < dayEnd);
            }

            //Orden de la lista
            var data = await qry
                .OrderBy(b => b.IdEvento)
                .Take(take)
                .ToListAsync(ct);

            // filtros para la vista
            ViewBag.Tabla = tabla;
            ViewBag.Tipo = tipo;
            ViewBag.Q = q;
            ViewBag.Take = take;
            ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd"); 

            return View(data);
        }
    }
}