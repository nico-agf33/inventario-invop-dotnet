using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Services;
using Proyect_InvOperativa.Dtos.OrdenCompra;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdenCompraDetalleController : ControllerBase
    {
        private readonly DetalleOrdenCompraService _detalleOCService;

        public OrdenCompraDetalleController(DetalleOrdenCompraService detalleOCService)
        {
            _detalleOCService = detalleOCService;
        }

            [HttpPost("crear")]
            public async Task<ActionResult> CrearDetalleOC([FromBody] OrdenCompraDetalleABMdto dto)
            {
                try
                {
                    var (advertencia, _) = await _detalleOCService.CreateDetalleOC(dto);
                    return Ok(new { advertencia }); 
                }catch (Exception ex){return BadRequest(new { error = ex.Message });}
            }

        [HttpPut("modificar")]
        public async Task<IActionResult> ModificarDetalle([FromBody] OrdenCompraDetalleABMdto dto)
        {
            try
            {
                await _detalleOCService.ModDetalleOC(dto);
                return Ok("detalle de orden de compra modificado correctamente ");
            }
            catch (Exception ex)
            {
                return BadRequest($"error al modificar detalle: {ex.Message}");
            }
        }

        [HttpDelete("eliminar")]
        public async Task<IActionResult> EliminarDetalle([FromBody] OrdenCompraDetalleABMdto dto)
        {
            try
            {
                await _detalleOCService.DeleteDetalleOC(dto);
                return Ok("detalle de orden de compra eliminado correctamente ");
            }
            catch (Exception ex)
            {
                return BadRequest($"error al eliminar detalle: {ex.Message}");
            }
        }
    }
}
