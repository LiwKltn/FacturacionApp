using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FacturacionApp.Data;
using FacturacionApp.Models;
using System;
using System.Collections.Generic;
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

        
        public async Task<IActionResult> Index()
        {
            try
            {
                var facturas = await _context.Facturas
                    .Include(f => f.Cliente)
                        .ThenInclude(c => c.Direccion) 
                    .Include(f => f.Empresa)
                    .Include(f => f.Lineas)
                        .ThenInclude(l => l.Producto) 
                    .AsSplitQuery() 
                    .OrderByDescending(f => f.Fecha)
                    .ThenBy(f => f.Numero)
                    .AsNoTracking()
                    .ToListAsync();

                // DEBUG: Verifica en consola
                Console.WriteLine($"Se encontraron {facturas.Count} facturas");
                if (facturas.Any())
                {
                    Console.WriteLine($"Primera factura: {facturas[0].Numero} con {facturas[0].Lineas.Count} líneas");
                }

                return View(facturas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                TempData["ErrorMessage"] = "Error al cargar facturas";
                return View(new List<Factura>());
            }
        }

        
        public IActionResult Create()
        {
            try
            {
                var clientes = _context.Clientes.AsNoTracking().OrderBy(c => c.Nombre).ToList();
                var empresas = _context.Empresas.AsNoTracking().OrderBy(e => e.Nombre).ToList();

                if (!clientes.Any() || !empresas.Any())
                {
                    TempData["ErrorMessage"] = "Debe existir al menos un cliente y una empresa registrados";
                    return RedirectToAction(nameof(Index));
                }

                var nuevaFactura = new Factura
                {
                    Numero = GenerarNumeroFactura(),
                    Fecha = DateTime.Today,
                    ClienteId = clientes.First().Id,
                    EmpresaId = empresas.First().Id,
                    Cliente = clientes.First(),
                    Empresa = empresas.First(),
                    Lineas = new List<LineaFactura>()
                };

                ViewBag.Clientes = new SelectList(clientes, "Id", "Nombre");
                ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");

                return View(nuevaFactura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación de factura");
                TempData["ErrorMessage"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numero,Fecha,ClienteId,EmpresaId,Lineas")] Factura factura)
        {
            try
            {
                
                if (factura.Fecha > DateTime.Today.AddDays(30))
                {
                    ModelState.AddModelError("Fecha", "La fecha no puede ser más de 30 días en el futuro");
                }

                
                var cliente = await _context.Clientes.FindAsync(factura.ClienteId);
                var empresa = await _context.Empresas.FindAsync(factura.EmpresaId);

                if (cliente == null || empresa == null)
                {
                    ModelState.AddModelError("", "Cliente o Empresa no válidos");
                }

                if (ModelState.IsValid)
                {
                    
                    factura.Cliente = cliente!;
                    factura.Empresa = empresa!;
                    factura.Lineas ??= new List<LineaFactura>();

                    
                    if (!factura.Lineas.Any())
                    {
                        ModelState.AddModelError("", "Debe agregar al menos una línea de factura");
                    }
                    else
                    {
                        using var transaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            _context.Add(factura);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            TempData["SuccessMessage"] = "Factura creada exitosamente";
                            return RedirectToAction(nameof(Index));
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Error al guardar factura");
                            TempData["ErrorMessage"] = "Error al guardar la factura en la base de datos";
                        }
                    }
                }

                
                ViewBag.Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "Nombre", factura.ClienteId);
                ViewBag.Empresas = new SelectList(await _context.Empresas.ToListAsync(), "Id", "Nombre", factura.EmpresaId);
                return View(factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear factura");
                TempData["ErrorMessage"] = "Error crítico al procesar la factura";
                return RedirectToAction(nameof(Index));
            }
        }

        private string GenerarNumeroFactura()
        {
            var ultimoNumero = _context.Facturas
                .OrderByDescending(f => f.Numero)
                .Select(f => f.Numero)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(ultimoNumero))
            {
                return "FAC-0001";
            }

            var numero = int.Parse(ultimoNumero.Split('-')[1]) + 1;
            return $"FAC-{numero:D4}";
        }
    }
}