using Microsoft.AspNetCore.Mvc;

namespace ReservaCancha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                estado = "disponible",
                timestamp = DateTime.UtcNow,
                mensaje = "El sistema está en funcionamiento."
            });
        }
    }
}