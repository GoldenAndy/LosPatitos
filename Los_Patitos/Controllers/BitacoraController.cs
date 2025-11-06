using Los_Patitos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Los_Patitos.Controllers
{
    public class BitacoraController : Controller
    {
        private readonly AppDbContext _db;
        public BitacoraController(AppDbContext db) => _db = db;

        // GET: /Bitacora
        // Lista para bitácora con filtros por tabla, tipo y texto
        public async Task<IActionResult> Index(string? tabla, string? tipo, string? q, int take = 500, CancellationToken ct = default)
        {
            // query sin tracking para lectura rápida
            var qry = _db.BITACORA_EVENTOS.AsNoTracking();

            // para filtrar el nombre de tabla exacto
            if (!string.IsNullOrWhiteSpace(tabla))
                qry = qry.Where(b => b.TablaDeEvento == tabla);
            
            // para filtrar el tipo de evento con el dropdown del index
            if (!string.IsNullOrWhiteSpace(tipo))
                qry = qry.Where(b => b.TipoDeEvento == tipo);

            if (!string.IsNullOrWhiteSpace(q))
            {
            // para filtrar por texto en descripción o JSON, para buscar IDs o palabras clave
                qry = qry.Where(b =>
                    b.DescripcionDeEvento.Contains(q) ||
                    (b.DatosAnteriores != null && b.DatosAnteriores.Contains(q)) ||
                    (b.DatosPosteriores != null && b.DatosPosteriores.Contains(q)));
            }

            // para que en la lista todo se acomode por IdEvento ascendente
            var data = await qry
                .OrderBy(b => b.IdEvento)
                .ToListAsync(ct);

            // guarda los filtros en el viewbag para poder usarlos en la vista (osea en el index)
            ViewBag.Tabla = tabla;
            ViewBag.Tipo = tipo;
            ViewBag.Q = q;
            ViewBag.Take = take;

            return View(data);
        }
    }
}