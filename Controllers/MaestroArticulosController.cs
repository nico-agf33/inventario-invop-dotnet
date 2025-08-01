﻿using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Dtos.MaestroArticulo;
using Proyect_InvOperativa.Dtos.Proveedor;
using Proyect_InvOperativa.Services;
using Proyect_InvOperativa.Utils;

namespace Proyect_InvOperativa.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MaestroArticulosController : ControllerBase
    {
        public readonly MaestroArticulosService _maestroArticulosService;

        public MaestroArticulosController(MaestroArticulosService masterArt)
        {
            _maestroArticulosService = masterArt;
        }

        #region Artículo

        [HttpPost("articulo/CreateArticulo")]
        public async Task<IActionResult> CreateArticulo([FromBody] ArticuloDto articuloDto)
        {
            var result = await _maestroArticulosService.CreateArticulo(articuloDto);

            return Ok(result);

        }

	[HttpDelete("articulo/DeleteArticulo/{idArticulo}")]
	public async Task<IActionResult> DeleteArticulo(long idArticulo)
	{
	    try
	    {
		await _maestroArticulosService.DeleteArticulo(idArticulo);
		return Ok(new { mensaje = "articulo eliminado correctamente" });
	    }catch (Exception ex){return BadRequest(new { mensaje = ex.Message });}
	}

        [HttpPost("articulo/UpdateArticulo")]
        public async Task<IActionResult> UpdateArticulo([FromBody] ArticuloDto articuloDto)
        {
            await _maestroArticulosService.UpdateArticulo(articuloDto);

            return Ok("Artículo modificado. ");

        }

        [HttpGet("articulo/GetAllArticulos")]
        public async Task<IActionResult> GetAllArticulos()
        {
            var articulos = await _maestroArticulosService.GetAllArticulos();

            return Ok(articulos);
        }

        [HttpGet("articulo/GetArticuloById/{idArticulo}")]
        public async Task<IActionResult> GetArticuloById(long idArticulo)
        {
            var articulo = await _maestroArticulosService.GetArticuloById(idArticulo);

            return Ok(articulo);
        }

        #endregion

        #region MaestroArticulo 
        [HttpPost("maestroarticulo/CreateMaestro")]
        public async Task<IActionResult> CreateMaestroArticulo([FromBody] CreateMaestroArticuloDto createMaestroArticuloDto)
        {
            var maestro = await _maestroArticulosService.CreateMaestroArticulo(createMaestroArticuloDto);

            return Ok(maestro);

        }

        [HttpDelete("maestroarticulo/DeleteMaestro/{idMaestroArticulo}")]
        public async Task<IActionResult> DeleteMaestroArticulo(long idMaestroArticulo)
        {
            await _maestroArticulosService.DeleteMaestroArticulo(idMaestroArticulo);

            return Ok("Maestro Artículo dado de baja. ");
        }
        #endregion

        #region Modelo Inventario


            [HttpGet("modeloInventario/calc-mod-inv")]
            public async Task<ActionResult<List<ArticuloInvDto>>> CalculoModInv()
            {
                var listaArt = await _maestroArticulosService.CalculoModInv();
                return Ok(listaArt ?? new List<ArticuloInvDto>());
            }

        [HttpGet("modeloInventario/control-stock-periodico")]
        public async Task<IActionResult> ControlStockPeriodico(CancellationToken cancellationToken)
        {
            await _maestroArticulosService.ControlStockPeriodico(cancellationToken);
            return NoContent();
        }

        [HttpGet("modeloInventario/lista-modelos")]
        public ActionResult<IEnumerable<object>> GetModelosInventario()
        {
            var modelosInv = _maestroArticulosService.GetModelosInventario();
            return Ok(modelosInv);
        } 

        [HttpGet("modeloInventario/lista-unidades-temp")]
        public ActionResult<IEnumerable<object>> GetUnidadesTemp()
        {
            var unidadesTemp = _maestroArticulosService.GetUnidadesTemp();
            return Ok(unidadesTemp);
        }         

        #endregion

        #region Proveedor Predeterminado
        [HttpPost("proveedor/predeterminado")]
        public async Task<IActionResult> EstablecerProveedorPredeterminado(long idArticulo, long idProveedor)
        {
            var mensaje = await _maestroArticulosService.EstablecerProveedorPredeterminadoAsync(idArticulo, idProveedor);
            return Ok(mensaje);
        }
        #endregion

        #region Artículos Listas
        [HttpGet("articulosLista/reponer")]
        public async Task<ActionResult<List<ArticuloStockReposicionDto>>> ListarArticulosAReponer()
        {
            var resultado = await _maestroArticulosService.ListarArticulosAReponer();
            return Ok(resultado);
        }

        [HttpGet("articulosLista/faltantes")]
        public async Task<ActionResult<List<ArticuloStockReposicionDto>>> ListarArticulosFaltantes()
        {
            var resultado = await _maestroArticulosService.ListarArticulosFaltantes();
            return Ok(resultado);
        }

        [HttpGet("articulosLista/proveedores/{idArticulo}")]
        public async Task<ActionResult<List<ProveedoresPorArticuloDto>>> ListarProveedoresPorArticulo(long idArticulo)
        {
            var resultado = await _maestroArticulosService.ListarProveedoresPorArticulo(idArticulo);
            return Ok(resultado);
        }

            [HttpGet("articulos/list-art-datos")]
            public async Task<ActionResult<List<ArticuloInvDto>>> ListarArticulosYDatos()
            {
                var listaArtD = await _maestroArticulosService.ListarArticulosYDatos();
                return Ok(listaArtD ?? new List<ArticuloInvDto>());
            }

                [HttpGet("calcular-cantidad-subtotal/{idArticulo}")]
                public async Task<ActionResult<ResultadoCantidadDto>> CalcularCantidadYSubtotal(long idArticulo)
                {
                    try
                    {
                        var resultado = await _maestroArticulosService.CalcularCantidadYSubtotal(idArticulo);
                        return Ok(resultado);
                    } catch (Exception ex){return BadRequest(new { error = ex.Message }); }
                }

                [HttpPut("ajuste-stock")]
                public async Task<IActionResult> AjusteInventario([FromBody] ArticuloInvDto dto)
                {
                    try
                    {
                        await _maestroArticulosService.AjusteInventarioAsync(dto);
                        return Ok(new { mensaje = "inventario actualizado correctamente " });
                    } catch (Exception ex) { return BadRequest(new { error = ex.Message });  }
                }

                [HttpGet("articulo-datos/{idArticulo}")]
                public async Task<IActionResult> GetArticuloYDatos(long idArticulo)
                {
                    try
                    {
                        var dto = await _maestroArticulosService.GetArticuloYDatos(idArticulo);
                        if (dto == null) return NotFound($"no se encuentra el articulo con Id {idArticulo} ");
                        return Ok(dto);
                    } catch (Exception ex) { return StatusCode(500, $"error al obtener datos del articulo: {ex.Message}");}
                }
        #endregion

        #region test funcion  ObtenerZ
        [HttpGet("test-z")]
        public ActionResult<double> ObtenerZTest(double nivelServicio)
        {
            try
            {
                return Ok(ModInventarioUtils.ObtenerZ(nivelServicio));
            }catch (Exception ex){return BadRequest(new { error = ex.Message });}
        }
        #endregion


    }
}
