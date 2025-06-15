using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FacturacionApp.Data;
using FacturacionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                var facturas = await _context.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Empresa)
                    .Include(f => f.Lineas)
                    .OrderByDescending(f => f.Fecha)
                    .ThenBy(f => f.Numero)
                    .ToListAsync();

                return View(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de facturas");
                TempData["ErrorMessage"] = "Error al cargar el listado de facturas";
                return View(new List<Factura>());
            }
        }

        // GET: Facturas/Create
        public IActionResult Create()
        {
            try
            {
                var clientes = _context.Clientes.OrderBy(c => c.Nombre).ToList();
                var empresas = _context.Empresas.OrderBy(e => e.Nombre).ToList();

                if (!clientes.Any() || !empresas.Any())
                {
                    TempData["ErrorMessage"] = "Debe existir al menos un cliente y una empresa registrados";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Clientes = new SelectList(clientes, "Id", "Nombre");
                ViewBag.Empresas = new SelectList(empresas, "Id", "Nombre");

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

                return View(nuevaFactura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación de factura");
                TempData["ErrorMessage"] = "Error al cargar el formulario de creación";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Facturas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Factura factura)
        {
            try
            {
                if (factura.Fecha > DateTime.Today.AddDays(30))
                {
                    ModelState.AddModelError("Fecha", "La fecha no puede ser más de 30 días en el futuro");
                }

                factura.Cliente = await _context.Clientes.FindAsync(factura.ClienteId);
                factura.Empresa = await _context.Empresas.FindAsync(factura.EmpresaId);

                if (factura.Cliente == null || factura.Empresa == null)
                {
                    ModelState.AddModelError("", "Cliente o Empresa no válidos");
                }

                if (ModelState.IsValid)
                {
                    factura.Lineas ??= new List<LineaFactura>();

                    if (!factura.Lineas.Any())
                    {
                        ModelState.AddModelError("", "Debe agregar al menos una línea de factura");
                        ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nombre", factura.ClienteId);
                        ViewBag.Empresas = new SelectList(_context.Empresas, "Id", "Nombre", factura.EmpresaId);
                        return View(factura);
                    }

                    _context.Add(factura);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Factura creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nombre", factura.ClienteId);
                ViewBag.Empresas = new SelectList(_context.Empresas, "Id", "Nombre", factura.EmpresaId);
                return View(factura);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar factura");
                ModelState.AddModelError("", "No se pudo guardar la factura. Intente nuevamente.");
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nombre", factura.ClienteId);
                ViewBag.Empresas = new SelectList(_context.Empresas, "Id", "Nombre", factura.EmpresaId);
                return View(factura);
            }
        }

        private string GenerarNumeroFactura()
        {
            try
            {
                var ultimoNumero = _context.Facturas
                    .OrderByDescending(f => f.Id)
                    .Select(f => f.Numero)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(ultimoNumero))
                {
                    return "FAC-0001";
                }

                var partes = ultimoNumero.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int numero))
                {
                    return $"FAC-{(numero + 1):D4}";
                }

                return $"FAC-{DateTime.Now:yyyyMMdd}-001";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de factura");
                return $"FAC-{DateTime.Now:yyyyMMddHHmmss}";
            }
        }
    }
}