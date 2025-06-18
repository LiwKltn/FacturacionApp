using FacturacionApp.Data;
using FacturacionApp.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SeedController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> SeedData()
    {
        if (!_context.Clientes.Any())
        {
            _context.Clientes.AddRange(
    new Cliente
    {
        Nombre = "Cliente 1",
        NIF = "00000000X" 
    },
    new Cliente
    {
        Nombre = "Cliente 2",
        NIF = "00000001Y" 
    }
);

            _context.Empresas.AddRange(
                new Empresa { Nombre = "Empresa A" },
                new Empresa { Nombre = "Empresa B" }
            );

            await _context.SaveChangesAsync();
            return Ok("Datos iniciales creados");
        }

        return Ok("Ya existen datos iniciales");
    }
}