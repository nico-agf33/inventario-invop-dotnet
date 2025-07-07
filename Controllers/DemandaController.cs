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

        [HttpGet("calcular-demanda")]
        public async Task<IActionResult> CalcularDemanda(
            [FromQuery] long idArticulo, 
            [FromQuery] long tipoPrediccion, 
            [FromQuery] long periodo, 
            [FromQuery] double? alfa)
        {
            try
            {
                if (tipoPrediccion == 3 && (!alfa.HasValue || alfa <= 0 || alfa >= 1)) return BadRequest(new { mensaje = "debe especificarse un valor de alfa entre 0 y 1 para la suavizacion exponencial " });
                var resultado = await _demandaService.CalcDemandaYDesviacion(idArticulo, tipoPrediccion, periodo, alfa ?? 0);
                return Ok(resultado);
            }catch (Exception ex){return BadRequest(new { mensaje = ex.Message }); }
        }
    }
}
