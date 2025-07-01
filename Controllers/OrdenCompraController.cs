using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.OrdenCompra;
using Proyect_InvOperativa.Services;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdenCompraController: ControllerBase
    {

        private readonly OrdenCompraService _ordenCompraService;
        public OrdenCompraController(OrdenCompraService ordenCompraService)
        {
            _ordenCompraService = ordenCompraService;
        }


    [HttpPost("generar-orden")]
    public async Task<IActionResult> GenerarOrdenCompra([FromBody] OrdenCompraGeneradaDto ordenC_Pedidos)
    {   
        try
        {
            var resultadoOC = await _ordenCompraService.GenerarOrdenCompra(ordenC_Pedidos.articulos, ordenC_Pedidos.idProveedor);
            return Ok(resultadoOC);
        }
        catch (Exception ex){return BadRequest(new {error = ex.Message});}
}

    [HttpPut("modificar-orden")]
    public async Task<IActionResult> ModificarOrdenCompra([FromBody] OrdenCompraModificadaDto ordenModDto)
    {   
        try
        {
            var resultadoOCM = await _ordenCompraService.ModificarOrdenCompra(ordenModDto);
            return Ok(resultadoOCM);
        }
        catch (Exception ex){return BadRequest(new {error = ex.Message});}
    
    }

    [HttpPost("confirmar-orden/{nOrdenCompra}")]
    public async Task<IActionResult> ConfirmarOrdenCompra(long nOrdenCompra)
    {
        try
        {
            await _ordenCompraService.ConfirmarOrdenCompra(nOrdenCompra);
            return Ok(new {mensaje = " orden de compra confirmada y enviada correctamente " });
        }
        catch (Exception ex){return BadRequest(new {error = ex.Message});}
    }

        [HttpPost("cancelar/{nOrdenCompra}")]
        public async Task<IActionResult> CancelarOrdenCompra(long nOrdenCompra)
        {
            await _ordenCompraService.CancelarOrdenCompra(nOrdenCompra);
            return Ok("orden de compra cancelada exitosamente ");
        }

        [HttpPost("registrar-entrada/{nOrdenCompra}")]
        public async Task<IActionResult> RegistrarEntradaPedido(long nOrdenCompra)
        {
            await _ordenCompraService.RegistrarEntradaPedido(nOrdenCompra);
            return Ok("entrada de articulos registrada correctamente ");
        }

        [HttpGet("prov-ord/{idProveedor}")]
        public async Task<ActionResult<List<OrdenCompraDto>>> GetOrdenesPorProveedor(long idProveedor)
        {
                var ordenesP = await _ordenCompraService.GetOrdenesPorProveedor(idProveedor);
                return Ok(ordenesP);
        }

        [HttpGet("lista-ordenes")]
        public async Task<IActionResult> GetOrdenesCompraLista()
        {
            var listaOC = await _ordenCompraService.GetOrdenesCompraLista();
            return Ok(listaOC);
        }

        [HttpGet("detalles-orden/{nOrdenCompra}")]
        public async Task<IActionResult> GetDetallesOrdenCompra(long nOrdenCompra)
        {
            try
            {
                var detallesDto = await _ordenCompraService.GetDetallesByOrdenId(nOrdenCompra);
                return Ok(detallesDto);
            }catch (Exception ex){return BadRequest(new { error = ex.Message });}
            }

            [HttpGet("proveedor/{nOrdenCompra}/articulos-no-inc/{idProveedor}")]
            public async Task<IActionResult> GetArticulosFaltantesEnOrden(long nOrdenCompra, long idProveedor)
            {
                var articulos = await _ordenCompraService.GetArticulosFaltantesEnOrden(nOrdenCompra, idProveedor);
                return Ok(articulos);
            }

            [HttpGet("articulo/ordenes/{idArticulo}")]
            public async Task<IActionResult> GetOrdenesPorArticulo(long idArticulo)
            {
                var ordenes = await _ordenCompraService.GetOrdenesPorArticulo(idArticulo);
                return Ok(ordenes);
            }

            [HttpGet("ordenCompra/{nOrdenCompra}/articulo/{idArticulo}")]
            public async Task<IActionResult> GetDetalleOrdenArticulo(long nOrdenCompra, long idArticulo)
            {   
                    var detalleDto = await _ordenCompraService.GetDetalleByOrdenYArticulo(nOrdenCompra, idArticulo);
                    return Ok(detalleDto);
            }

            [HttpGet("validar-detalles/{nOrdenCompra}")]
            public async Task<ActionResult<OrdenCompraAvisoDto>> ValidarDetallesOrdenCompra(long nOrdenCompra)
            {
                try
                {
                    var resultado = await _ordenCompraService.ValidarOrdenCompraExistente(nOrdenCompra);
                    return Ok(resultado);
                }catch (Exception ex){return BadRequest(new { error = ex.Message });}
            }


            [HttpPut("ordenCompra/{nOrdenCompra}/cambiar-proveedor/{idProveedor}")]
            public async Task<IActionResult> CambiarProveedorOrdenCompra(long nOrdenCompra, long idProveedor)
            {
            
            try {
             await _ordenCompraService.CambiarProveedor(nOrdenCompra, idProveedor);
             return Ok(new { mensaje = "proveedor actualizado correctamente para la orden de compra " });
            } catch (Exception ex){return BadRequest(new { error = ex.Message });}
            
            }
    }
}
