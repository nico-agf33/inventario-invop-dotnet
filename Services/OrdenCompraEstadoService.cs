using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Dtos;
using Proyect_InvOperativa.Dtos.OrdenCompra;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Services
{
    public class OrdenCompraEstadoService
    {
        public OrdenCompraEstadoRepository _OCrepository;
        public OrdenCompraEstadoService(OrdenCompraEstadoRepository ordencita)
        {
            _OCrepository = ordencita;
        }


        public async Task<OrdenCompraEstado> CreateOrdenCompraEstado(OrdenCompraEstadosDto dtoOrdenEstado)
        {
            var ordenEstadoNueva = new OrdenCompraEstado()
            {
                nombreEstadoOrden = dtoOrdenEstado.nombreEstadoOrden,
                idOrdenCompraEstado = dtoOrdenEstado.idOrdenCompraEstado,
                fechaFinEstadoDisponible = null,
            };
            return await _OCrepository.AddAsync(ordenEstadoNueva);
        }

        public async Task DeleteOrdenCompraEstado(long id)
        {
            var ordenCEstado = await _OCrepository.GetByIdAsync(id);

            ordenCEstado.fechaFinEstadoDisponible = DateTime.UtcNow;

            await _OCrepository.UpdateAsync(ordenCEstado);
        }

        public async Task<List<OrdenCompraEstadosDto>> ListarEstadosOrdenCompra()
        {
            var estados = await _OCrepository.GetAllEstadosOrden();
            return estados.Select(est => new OrdenCompraEstadosDto
            {
                idOrdenCompraEstado = est.idOrdenCompraEstado,
                nombreEstadoOrden = est.nombreEstadoOrden,
                fechaFinEstadoDisponible = est.fechaFinEstadoDisponible
            }).ToList();
        }
     
    }
}
