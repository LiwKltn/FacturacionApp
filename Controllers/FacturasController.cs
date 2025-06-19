using FacturacionApp.Data;
using FacturacionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
                    .ThenInclude(l => l.Producto)
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
            // Generar número de factura automático
            var ultimaFactura = await _context.Facturas
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            var siguienteNumero = "FAC-0001"; // Formato: FAC-0001, FAC-0002, etc.

            if (ultimaFactura != null && !string.IsNullOrEmpty(ultimaFactura.Numero))
            {
                try
                {
                    // Extraer la parte numérica del último número
                    var partes = ultimaFactura.Numero.Split('-');
                    if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNumero))
                    {
                        siguienteNumero = $"FAC-{(ultimoNumero + 1).ToString("D4")}";
                    }
                }
                catch
                {
                    // Si hay algún error en el formato, continuar con la numeración por ID
                    siguienteNumero = $"FAC-{ultimaFactura.Id + 1:D4}";
                }
            }

            // Crear nueva factura con número generado
            var factura = new Factura
            {
                Numero = siguienteNumero,
                Fecha = DateTime.Today,
                Lineas = new List<LineaFactura>()
            };

            // Cargar datos para los dropdowns
            ViewBag.Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre");
            ViewBag.Empresas = new SelectList(await _context.Empresas.ToListAsync(), "Id", "Nombre");
            ViewBag.Productos = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre");

            return View(factura);
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
            // Validar que el número de factura sea único
            if (await _context.Facturas.AnyAsync(f => f.Numero == factura.Numero))
            {
                ModelState.AddModelError("Numero", "Este número de factura ya existe");
            }

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
            ViewBag.Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre");
            ViewBag.Empresas = new SelectList(await _context.Empresas.ToListAsync(), "Id", "Nombre");
            ViewBag.Productos = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre");

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

            ViewBag.Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre");
            ViewBag.Empresas = new SelectList(await _context.Empresas.ToListAsync(), "Id", "Nombre");
            ViewBag.Productos = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre");

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

            // Recargar datos si hay error
            ViewBag.Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre");
            ViewBag.Empresas = new SelectList(await _context.Empresas.ToListAsync(), "Id", "Nombre");
            ViewBag.Productos = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre");

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