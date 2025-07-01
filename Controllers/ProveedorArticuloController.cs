using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Services;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorArticuloController : ControllerBase
    {
        private readonly ProveedorArticuloService _proveedorArticuloService;

        public ProveedorArticuloController(ProveedorArticuloService proveedorArticuloService)
        {
            _proveedorArticuloService = proveedorArticuloService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProveedorArticulo([FromBody] ProveedorArticuloDto provArtDto)
        {
            var result = await _proveedorArticuloService.CreateProveedorArticulo(provArtDto);
            return Ok(result);
        }

        [HttpPatch("baja-prov-art")]
        public async Task<IActionResult> BajaProvArt([FromBody] ProveedorArticuloDto provArtDto)
            {
                try
               {
                await _proveedorArticuloService.DeleteProveedorArticulo(provArtDto);
                return NoContent(); 
               }
                catch (Exception ex){return BadRequest(new {error = ex.Message});}
            }

                [HttpPut("mod-prov-art")]
                public async Task<IActionResult> UpdateProveedorArticulo([FromBody] ProveedorArticuloDto paDto)
                {
                    try
                    {
                        await _proveedorArticuloService.UpdateProveedorArticulo(paDto);
                        return NoContent(); 
                    }
                    catch (Exception ex){return BadRequest(new {error = ex.Message});}
                }
        
        [HttpGet("art-no-relacionados/{idProveedor}")]
        public async Task<IActionResult> GetArticulosNoRelacionados(long idProveedor)
        {
            var articulosNR = await _proveedorArticuloService.ArticulosNoRelacionadosProv(idProveedor);
            return Ok(articulosNR);
        }

    }
}