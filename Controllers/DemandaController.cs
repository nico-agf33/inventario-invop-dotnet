using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Services;
using System;
using System.Threading.Tasks;

namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemandaController : ControllerBase
    {
        private readonly DemandaService _demandaService;
        public readonly MaestroArticulosService _maestroArticulosService;

        public DemandaController(DemandaService demandaService,MaestroArticulosService masterArt)
        {
            _demandaService = demandaService;
            _maestroArticulosService = masterArt;
        }

        [HttpGet("calc-demanda/{idArticulo}")]
        public async Task<IActionResult> CalcularDemandaYDesviacion(long idArticulo)
        {
            try
            {
                var articulo = await _maestroArticulosService.GetArticuloById(idArticulo);
                if (articulo == null) return NotFound(new { mensaje = $"articulo con Id {idArticulo} no encontrado " });

                var resultado = await _demandaService.CalcularDemandaYDesviacionAnual(articulo);
                return Ok(new
                {
                    demanda = resultado.demanda,
                    desviacionAnual = resultado.desviacionAnual
                });
            } catch (InvalidOperationException ex) {return BadRequest(new { mensaje = ex.Message });
            } catch (Exception ex){return StatusCode(500, new { mensaje = "error : " + ex.Message });}
        }

        [HttpGet("calc-errores/{idArticulo}")]
        public async Task<IActionResult> CalcularErroresDemanda(long idArticulo)
        {
            try
            {
                var articulo = await _maestroArticulosService.GetArticuloById(idArticulo);
                if (articulo == null) return NotFound(new { mensaje = $"articulo con Id {idArticulo} no encontrado " });

                var resultado = await _demandaService.CalcularErroresDemanda(articulo);
                return Ok(new
                {
                    rmse = resultado.rmse,
                    mae = resultado.mae,
                    mape = resultado.mape
                });
            } catch (InvalidOperationException ex) {return BadRequest(new { mensaje = ex.Message });
            } catch (Exception ex) {return StatusCode(500, new { mensaje = "error : " + ex.Message });}
        }
    }
}
