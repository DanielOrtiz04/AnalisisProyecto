using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Models;

namespace ReservaCancha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanchasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CanchasController(AppDbContext context)
        {
            _context = context;
        }

     
        [HttpGet]
        public async Task<IActionResult> GetCanchas()
        {
            var canchas = await _context.Canchas
                .Where(c => c.Disponible)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Tipo,
                    c.Precio,
                    c.Descripcion,
                    c.Disponible
                })
                .ToListAsync();

            return Ok(canchas);
        }
    }
}