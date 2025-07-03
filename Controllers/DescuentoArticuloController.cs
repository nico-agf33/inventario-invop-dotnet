using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.Ventas;
using Proyect_InvOperativa.Services;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DescuentoArticuloController : ControllerBase
    {
        private readonly DescuentoArticuloService _descuentoService;

        public DescuentoArticuloController(DescuentoArticuloService descuentoService)
        {
            _descuentoService = descuentoService;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> CrearDescuento([FromBody] DescuentoArticuloDto dto)
        {
            try
            {
                await _descuentoService.CrearDescuentoArticulo(dto);
                return Ok("descuento creado correctamente");
            } catch (Exception ex){ return BadRequest(ex.Message);}
        }

        [HttpPut("modificar")]
        public async Task<IActionResult> ModDescuento([FromBody] DescuentoArticuloDto dto)
        {
            try
            {
                await _descuentoService.ModDescuentoArticulo(dto);
                return Ok("descuento modificado correctamente ");
            } catch (Exception ex){return BadRequest(ex.Message); }
        }

        [HttpDelete("baja-logica/{idArticulo}")]
        public async Task<IActionResult> BajaDescuento(long idArticulo)
        {
            try
            {
                await _descuentoService.BajaDescuentoArticulo(idArticulo);
                return Ok("descuento eliminado correctamente");
            }catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("descuento-vigente/{idArticulo}")]
        public async Task<IActionResult> GetDescuentoVigentePorIdArticulo(long idArticulo)
        {
            try
            {
                var dto = await _descuentoService.GetDescuentoVigentePorIdArticulo(idArticulo);
                return Ok(new
                {
                    mensaje = "descuento vigente obtenido correctamente",
                    descuento = dto
                });
            } catch (Exception ex){
                return BadRequest(new
                {
                    mensaje = "no fue posible obtener el descuento vigente",
                    error = ex.Message
                });
            }
        }

    }
}