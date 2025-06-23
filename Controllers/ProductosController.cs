using FacturacionApp.Data;
using FacturacionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FacturacionApp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            return View(await _context.Productos.ToListAsync());
        }

        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Precio,IvaPorcentaje,Descripcion")] Producto producto)
        {
            if (await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo))
            {
                ModelState.AddModelError("Codigo", "Este código ya está registrado");
            }

            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Precio,IvaPorcentaje,Descripcion")] Producto producto)
        {
            if (id != producto.Id) return NotFound();

            
            if (await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo && p.Id != producto.Id))
            {
                ModelState.AddModelError("Codigo", "Este código ya está registrado");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            
            if (await _context.LineaFacturas.AnyAsync(lf => lf.ProductoId == id))
            {
                TempData["ErrorMessage"] = "No se puede eliminar: El producto está asociado a facturas";
                return RedirectToAction(nameof(Index));
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Producto eliminado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}