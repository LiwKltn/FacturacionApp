using FacturacionApp.Data;
using FacturacionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionApp.Controllers
{
    public class FacturasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FacturasController> _logger;

        public FacturasController(ApplicationDbContext context, ILogger<FacturasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Facturas
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Empresa)
                    .Include(f => f.Lineas)
                        .ThenInclude(l => l.Producto)
                    .OrderByDescending(f => f.Fecha)
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de facturas");
                return RedirectToAction(nameof(Error));
            }
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error));
            }

            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Empresa)
                    .Include(f => f.Lineas)
                        .ThenInclude(l => l.Producto)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (factura == null)
                {
                    return RedirectToAction(nameof(Error));
                }

                return View(factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ver detalles de factura ID: {id}");
                return RedirectToAction(nameof(Error));
            }
        }

        // GET: Facturas/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                await CargarListasDesplegables();
                return View(new Factura
                {
                    Numero = await GenerarNumeroFacturaAsync(),
                    Fecha = DateTime.Today
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación");
                return RedirectToAction(nameof(Error));
            }
        }

        // POST: Facturas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Numero,Fecha,ClienteId,EmpresaId")] Factura factura,
            List<int> productoIds,
            List<int> cantidades,
            List<decimal> precios)
        {
            if (productoIds == null || productoIds.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos una línea");
                await CargarListasDesplegables();
                return View(factura);
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            _context.Facturas.Add(factura);
                            await _context.SaveChangesAsync();

                            for (int i = 0; i < productoIds.Count; i++)
                            {
                                var producto = await _context.Productos.FindAsync(productoIds[i]);
                                if (producto == null) continue;

                                _context.LineaFacturas.Add(new LineaFactura
                                {
                                    FacturaId = factura.Id,
                                    ProductoId = productoIds[i],
                                    Cantidad = cantidades[i],
                                    PrecioUnitario = precios[i],
                                    IvaPorcentaje = producto.IvaPorcentaje
                                });
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            TempData["Success"] = "Factura creada correctamente";
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear factura");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                await CargarListasDesplegables();
                return View(factura);
            }
        }

        // GET: Facturas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error));
            }

            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Lineas)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (factura == null)
                {
                    return RedirectToAction(nameof(Error));
                }

                await CargarListasDesplegables();
                return View(factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar edición de factura ID: {id}");
                return RedirectToAction(nameof(Error));
            }
        }

        // POST: Facturas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Numero,Fecha,ClienteId,EmpresaId")] Factura factura,
            List<int> productoIds,
            List<int> cantidades,
            List<decimal> precios)
        {
            if (id != factura.Id)
            {
                return RedirectToAction(nameof(Error));
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var lineasExistentes = await _context.LineaFacturas
                                .Where(l => l.FacturaId == id)
                                .ToListAsync();
                            _context.LineaFacturas.RemoveRange(lineasExistentes);

                            for (int i = 0; i < productoIds.Count; i++)
                            {
                                var producto = await _context.Productos.FindAsync(productoIds[i]);
                                if (producto == null) continue;

                                _context.LineaFacturas.Add(new LineaFactura
                                {
                                    FacturaId = factura.Id,
                                    ProductoId = productoIds[i],
                                    Cantidad = cantidades[i],
                                    PrecioUnitario = precios[i],
                                    IvaPorcentaje = producto.IvaPorcentaje
                                });
                            }

                            _context.Update(factura);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            TempData["Success"] = "Factura actualizada correctamente";
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar factura ID: {id}");
                ModelState.AddModelError("", $"Error al guardar cambios: {ex.Message}");
                await CargarListasDesplegables();
                return View(factura);
            }
        }

        // GET: Facturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error));
            }

            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Empresa)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (factura == null)
                {
                    return RedirectToAction(nameof(Error));
                }

                return View(factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar eliminación de factura ID: {id}");
                return RedirectToAction(nameof(Error));
            }
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var factura = await _context.Facturas
                                .Include(f => f.Lineas)
                                .FirstOrDefaultAsync(f => f.Id == id);

                            if (factura != null)
                            {
                                _context.LineaFacturas.RemoveRange(factura.Lineas);
                                _context.Facturas.Remove(factura);
                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }

                            TempData["Success"] = "Factura eliminada correctamente";
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar factura ID: {id}");
                TempData["Error"] = "Error al eliminar la factura";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        private bool FacturaExists(int id)
        {
            return _context.Facturas.Any(e => e.Id == id);
        }

        private async Task<string> GenerarNumeroFacturaAsync()
        {
            try
            {
                var ultimaFactura = await _context.Facturas
                    .OrderByDescending(f => f.Id)
                    .FirstOrDefaultAsync();

                return ultimaFactura == null ? "FAC-0001" : $"FAC-{ultimaFactura.Id + 1:0000}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de factura");
                throw;
            }
        }

        private async Task CargarListasDesplegables()
        {
            try
            {
                ViewBag.Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre");
                ViewBag.Empresas = new SelectList(await _context.Empresas.ToListAsync(), "Id", "Nombre");
                ViewBag.Productos = await _context.Productos.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar listas desplegables");
                throw;
            }
        }
    }
}