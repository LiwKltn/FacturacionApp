using FacturacionApp.Data;
using FacturacionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionApp.Controllers
{
    public class FacturasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Facturas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Empresa)
                .ToListAsync());
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Empresa)
                .Include(f => f.Lineas)
                    .ThenInclude(l => l.Producto) // Carga los productos relacionados
                .FirstOrDefaultAsync(m => m.Id == id);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Facturas/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Clientes"] = await _context.Clientes.ToListAsync();
            ViewData["Empresas"] = await _context.Empresas.ToListAsync();
            ViewData["Productos"] = await _context.Productos.ToListAsync();
            return View();
        }

        // POST: Facturas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Numero,Fecha,ClienteId,EmpresaId")] Factura factura,
            int[] productoIds,
            int[] cantidades,
            decimal[] precios)
        {
            if (ModelState.IsValid)
            {
                factura.Lineas = new List<LineaFactura>();

                for (int i = 0; i < productoIds.Length; i++)
                {
                    factura.Lineas.Add(new LineaFactura
                    {
                        ProductoId = productoIds[i],
                        Cantidad = cantidades[i],
                        PrecioUnitario = precios[i],
                        IvaPorcentaje = await _context.Productos
                            .Where(p => p.Id == productoIds[i])
                            .Select(p => p.IvaPorcentaje)
                            .FirstOrDefaultAsync()
                    });
                }

                _context.Add(factura);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar datos si hay error
            ViewData["Clientes"] = await _context.Clientes.ToListAsync();
            ViewData["Empresas"] = await _context.Empresas.ToListAsync();
            ViewData["Productos"] = await _context.Productos.ToListAsync();
            return View(factura);
        }

        // GET: Facturas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null)
            {
                return NotFound();
            }

            ViewData["Clientes"] = await _context.Clientes.ToListAsync();
            ViewData["Empresas"] = await _context.Empresas.ToListAsync();
            ViewData["Productos"] = await _context.Productos.ToListAsync();
            return View(factura);
        }

        // POST: Facturas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Numero,Fecha,ClienteId,EmpresaId")] Factura factura,
            int[] productoIds,
            int[] cantidades,
            decimal[] precios)
        {
            if (id != factura.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Eliminar líneas existentes
                    var lineasExistentes = await _context.LineaFacturas
                        .Where(l => l.FacturaId == id)
                        .ToListAsync();
                    _context.LineaFacturas.RemoveRange(lineasExistentes);

                    // Agregar nuevas líneas
                    factura.Lineas = new List<LineaFactura>();
                    for (int i = 0; i < productoIds.Length; i++)
                    {
                        factura.Lineas.Add(new LineaFactura
                        {
                            FacturaId = id,
                            ProductoId = productoIds[i],
                            Cantidad = cantidades[i],
                            PrecioUnitario = precios[i],
                            IvaPorcentaje = await _context.Productos
                                .Where(p => p.Id == productoIds[i])
                                .Select(p => p.IvaPorcentaje)
                                .FirstOrDefaultAsync()
                        });
                    }

                    _context.Update(factura);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaExists(factura.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(factura);
        }

        // GET: Facturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Empresa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura != null)
            {
                _context.Facturas.Remove(factura);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FacturaExists(int id)
        {
            return _context.Facturas.Any(e => e.Id == id);
        }
    }
}