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

        [HttpPost("validar-stock")]
        public async Task<IActionResult> ValidarStock([FromBody] StockDto stockDto)
        {
            var disponible = await _ventasService.ValidarStockDisponible(stockDto);
            return Ok(disponible);
        }

        [HttpPost("crear-venta")]
        public async Task<IActionResult> CreateVentas([FromBody] VentasDto ventasDto)
        {
            try
            {
                var result = await _ventasService.CreateVentas(ventasDto);
                return Ok(new
                {
                    mensaje = "venta generada correctamente",
                    venta = result.venta,
                    advertencias = result.advertencias
                });
            } catch (Exception ex) {
                return BadRequest(new
                {
                    mensaje = "error al generar la venta: ",
                    error = ex.Message
                });
            }
        }

        [HttpGet("art-vent/{idArticulo}")]
        public async Task<ActionResult<List<ArtVentasDto>>> GetVentasPorArticulo(long idArticulo)
        {
             var listaArtVentas = await _ventasService.GetVentasPorArticulo(idArticulo);
             return Ok(listaArtVentas);
        }

        [HttpPost("visualizar-venta")]
        public async Task<ActionResult> MostrarDetallesDeVentaARegistrar([FromBody] VentasDto ventaDto)
        {
            try
            {
                var (ventaModificada, resultadoAdvertencias) = await _ventasService.MostrarDetallesDeVentaARegistrar(ventaDto);
                return Ok(new
                {
                    venta = ventaModificada,
                    resultado = resultadoAdvertencias
                });
            }  catch (Exception ex) { return BadRequest(new { mensaje = ex.Message }); }
        }

           [HttpPost("precio-venta-art")]
            public async Task<IActionResult> ActualizarPrecioVentaArt([FromBody] PrecioVentaArtDto dto)
            {
                try
                {
                    await _ventasService.ActualizarPrecioVentaArt(dto);
                    return Ok(new
                    {
                        mensaje = $"precio de venta actualizado correctamente para el articulo con Id {dto.idArticulo}"
                    });
                }  catch (Exception ex) {
                    return BadRequest(new
                    {
                        mensaje = "error al actualizar el precio de venta: ",
                        error = ex.Message
                    });
                }
            }

            [HttpGet("ver-precio-venta-articulo/{idArticulo}")]
            public async Task<IActionResult> GetPrecioVentaArticulo(long idArticulo)
            {
                try
                {
                    var precio = await _ventasService.GetPrecioVentaArticulo(idArticulo);
                    return Ok(new
                    {
                        mensaje = "precio de venta obtenido correctamente",
                        precioVenta = precio
                    });
                } catch (Exception ex) {
                    return BadRequest(new
                    {
                        mensaje = "error al obtener el precio de venta del artículo",
                        error = ex.Message
                    });
                }
            }
    }
}
