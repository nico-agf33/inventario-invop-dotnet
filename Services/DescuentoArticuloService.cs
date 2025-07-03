using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Dtos.Ventas;

namespace Proyect_InvOperativa.Services
{
    public class DescuentoArticuloService
    {
        private readonly ArticuloRepository _articuloRepository;
        private readonly StockArticuloRepository _stockArticuloRepository;
        private readonly DescuentoArticuloRepository _descuentoArticuloRepository;

        public DescuentoArticuloService(
            ArticuloRepository articuloRepository,
            StockArticuloRepository stockArticuloRepository,
            DescuentoArticuloRepository descuentoArticuloRepository)
        {
            _articuloRepository = articuloRepository;
            _stockArticuloRepository = stockArticuloRepository;
            _descuentoArticuloRepository = descuentoArticuloRepository;
        }

        public async Task CrearDescuentoArticulo(DescuentoArticuloDto dto)
        {
            var articulo = await _articuloRepository.GetArticuloById(dto.idArticulo);
            if (articulo == null)  throw new Exception($"articulo con Id {dto.idArticulo} no encontrado ");

            var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
            if (stock == null || stock.fechaStockFin != null)  throw new Exception($"el articulo con Id {dto.idArticulo} esta dado de baja ");

            var nuevoDescuento = new DescuentoArticulo
            {
                articulo = articulo,
                cantidadDesc = dto.cantidadDesc,
                porcentajeDesc = dto.porcentajeDesc,
                fechaIDescuento = DateTime.UtcNow,
                fechaFDescuento = null
            };

            await _descuentoArticuloRepository.AddAsync(nuevoDescuento);
        }

        public async Task ModDescuentoArticulo(DescuentoArticuloDto dto)
        {
            var descuentoActual = await _descuentoArticuloRepository.getdescActualbyIdArticulo(dto.idArticulo);

            if (descuentoActual == null) throw new Exception($"no existe un descuento vigente para el articulo con Id {dto.idArticulo}");

            descuentoActual.porcentajeDesc = dto.porcentajeDesc;
            descuentoActual.cantidadDesc = dto.cantidadDesc;

            await _descuentoArticuloRepository.UpdateAsync(descuentoActual);
        }

        public async Task BajaDescuentoArticulo(long idArticulo)
        {
            var descuentoActual = await _descuentoArticuloRepository.getdescActualbyIdArticulo(idArticulo);

            if (descuentoActual == null) throw new Exception($"no existe un descuento vigente para el articulo con Id {idArticulo}");

            descuentoActual.fechaFDescuento = DateTime.UtcNow;

            await _descuentoArticuloRepository.UpdateAsync(descuentoActual);
        }

        public async Task<DescuentoArticuloDto> GetDescuentoVigentePorIdArticulo(long idArticulo)
        {
            var descuento = await _descuentoArticuloRepository.getdescActualbyIdArticulo(idArticulo);
            if (descuento == null) throw new Exception($"no hay un descuento vigente para el articulo con Id {idArticulo}");

            return new DescuentoArticuloDto
            {
                idArticulo = idArticulo,
                cantidadDesc = descuento.cantidadDesc,
                porcentajeDesc = descuento.porcentajeDesc
            };
        }

    }
}
