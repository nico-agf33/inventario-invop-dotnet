using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Dtos.Proveedor;
using Proyect_InvOperativa.Services;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly ProveedorService _proveedorService;

        public ProveedorController(ProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> CreateProveedor([FromBody] ProveedorDto proveedorDto)
        {
            var result = await _proveedorService.CreateProveedor(proveedorDto);
            return Ok(result);
        }

        [HttpPut("actualizar/{idProveedor}")]
        public async Task<IActionResult> UpdateProveedor(long idProveedor, [FromBody] ProveedorDto proveedorDto)
        {
            await _proveedorService.UpdateProveedor(idProveedor, proveedorDto);
            return NoContent();
        }

        [HttpPut("suspender/{idProveedor}")]
        public async Task<IActionResult> SuspenderProveedor(long idProveedor)
        {
            try
            {
                await _proveedorService.SuspenderProveedor(idProveedor);
                return NoContent();
            }
            catch (Exception ex){return BadRequest(new { error = ex.Message }); }
}

        [HttpPut("restaurar/{idProveedor}")]
        public async Task<IActionResult> RestaurarProveedor(long idProveedor)
        {
            await _proveedorService.RestaurarProveedor(idProveedor);
            return NoContent();
        }

        [HttpDelete("eliminar/{idProveedor}")]
        public async Task<IActionResult> DeleteProveedor(long idProveedor)
        {
            try {
            await _proveedorService.DeleteProveedor(idProveedor);
            return NoContent(); 
            }
            catch (Exception ex){return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("{idProveedor}")]
        public async Task<IActionResult> GetProveedorById(long idProveedor)
        {
            var result = await _proveedorService.GetProveedorById(idProveedor);
            return Ok(result);
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllProveedores()
        {
            var result = await _proveedorService.GetAllProveedores();
            return Ok(result);
        }

        [HttpGet("proveedores-sist")]
        public async Task<ActionResult<List<ProveedorDto>>> GetProveedoresConDto()
        {
            var listaProvSist = await _proveedorService.GetProveedoresConDto();
            return Ok(listaProvSist);
        }

        [HttpGet("activos")]
        public async Task<ActionResult<List<ProveedorDto>>> GetProveedoresActivos()
        {
            var listaProvActivos = await _proveedorService.GetProveedoresActivos();
            return Ok(listaProvActivos);
        }

        [HttpGet("suspendidos")]
        public async Task<ActionResult<List<ProveedorDto>>> GetProveedoresSuspendidos()
        {
            var listaProvSuspendidos = await _proveedorService.GetProveedoresSuspendidos();
            return Ok(listaProvSuspendidos);
        }

        [HttpGet("articulos-proveedor/{idProveedor}")]
        public async Task<ActionResult<List<ProveedorArticuloDto>>> ListarArticulosProveedor(long idProveedor)
        {
                var articulosP = await _proveedorService.GetArticulosPorProveedor(idProveedor);
                return Ok(articulosP);
        }           

        [HttpPost("crea-prov-art")]
        public async Task<IActionResult> AltaProveedorConArticulos([FromBody] ProvConArticulosDto ProvArtDto)
        {
            try
            {
                var result = await _proveedorService.AltaProveedorConArticulos(ProvArtDto);
                return Ok(new { mensaje = result });
            }
            catch (Exception exc) {return BadRequest(new { error = exc.Message}); }
        }

        [HttpGet("{idProveedor}/historial")]
        public async Task<IActionResult> GetHistorialEstadosProveedor(long idProveedor)
        {
            var estadosP = await _proveedorService.GetHistoricoEstadosProveedor(idProveedor);
            return Ok(estadosP);
        }

            [HttpGet("articulo/prov-pred/{idArticulo}")]
            public async Task<IActionResult> GetProveedorPredeterminado(long idArticulo)
            {
            try {
             var idProveedorP = await _proveedorService.GetProvPredeterminadoArt(idArticulo);
             return Ok(idProveedorP);
            } 
            catch (Exception exc) {return BadRequest(new { error = exc.Message}); }

            }
    }
}
