using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Proyect_InvOperativa.Dtos.OrdenCompra;
using Proyect_InvOperativa.Services;
using System;
namespace Proyect_InvOperativa.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdenCompraEstadoController: ControllerBase
    {


        private readonly OrdenCompraEstadoService _ordenCompraEstadoService;
        public OrdenCompraEstadoController(OrdenCompraEstadoService ordenCompraEstadoService)
        {
            _ordenCompraEstadoService = ordenCompraEstadoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrdenCompraEstados([FromBody] OrdenCompraEstadosDto ordenCompraEstadoDto)
        {
            var result = await _ordenCompraEstadoService.CreateOrdenCompraEstado(ordenCompraEstadoDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrdenCompraEstado(long id)
        {
            await _ordenCompraEstadoService.DeleteOrdenCompraEstado(id);
            return NoContent();
        }

        [HttpGet("estados-orden")]
        public async Task<IActionResult> GetEstadosOrdenCompra()
        {
            var estadosOC = await _ordenCompraEstadoService.ListarEstadosOrdenCompra();
            return Ok(estadosOC);
        }

    } 
}
