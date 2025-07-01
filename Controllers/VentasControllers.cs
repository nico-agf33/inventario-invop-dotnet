using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.Ventas;
using Proyect_InvOperativa.Services;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly VentasService _ventasService;

        public VentasController(VentasService ventasService)
        {
            _ventasService = ventasService;
        }

        [HttpGet("validar-stock")]
        public async Task<IActionResult> ValidarStock([FromBody] StockDto ventasDto)
        {
            var disponible = await _ventasService.ValidarStockDisponible(ventasDto);
            return Ok(disponible);
        }

            [HttpPost("crear-venta")]
            public async Task<IActionResult> CreateVentas([FromBody] VentasDto ventasDto)
            {
                var result = await _ventasService.CreateVentas(ventasDto);
                return Ok(new {
                    mensaje = "venta generada correctamente",
                    venta = result.venta,
                    advertencias = result.advertencias
                });
}

        [HttpGet("art-vent/{idArticulo}")]
        public async Task<ActionResult<List<ArtVentasDto>>> GetVentasPorArticulo(long idArticulo)
        {
             var listaArtVentas = await _ventasService.GetVentasPorArticulo(idArticulo);
             return Ok(listaArtVentas);
        }

            [HttpPost("visualizar-venta")]
            public async Task<ActionResult<VentasDto>> MostrarDetallesDeVentaARegistrar([FromBody] VentasDto ventaDto)
            {
                try
                {
                    var resultado = await _ventasService.MostrarDetallesDeVentaARegistrar(ventaDto);
                    return Ok(resultado);
                }catch (Exception ex){return BadRequest(new { mensaje = ex.Message }); }
           }
    }
}
