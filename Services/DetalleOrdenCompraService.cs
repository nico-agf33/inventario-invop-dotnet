using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Dtos.OrdenCompra;
using Proyect_InvOperativa.Models.Enums;

namespace Proyect_InvOperativa.Services
{
    public class DetalleOrdenCompraService
    {
        private readonly DetalleOrdenCompraRepository _detalleOrdenCompraRepository;
        private readonly OrdenCompraRepository _ordenCompraRepository;
        private readonly ArticuloRepository _articuloRepository;
        private readonly ProveedorArticuloService _proveedorArticuloService;
        private readonly ProveedorArticuloRepository _proveedorArticuloRepository;

        public DetalleOrdenCompraService(
            DetalleOrdenCompraRepository detalleOrdenCompraRepository,
            OrdenCompraRepository ordenCompraRepository,
            ProveedorArticuloService proveedorArticuloService,
            ArticuloRepository articuloRepository,
            ProveedorArticuloRepository proveedorArticuloRepository)
        {
            _detalleOrdenCompraRepository = detalleOrdenCompraRepository;
            _ordenCompraRepository = ordenCompraRepository;
            _articuloRepository = articuloRepository;
            _proveedorArticuloService = proveedorArticuloService;
            _proveedorArticuloRepository = proveedorArticuloRepository;
        }

            public async Task<(string? advertencia, DetalleOrdenCompra? detalleGenerado)> CreateDetalleOC(OrdenCompraDetalleABMdto dto)
            {
                var orden = await _ordenCompraRepository.GetOrdenCompraConEstado(dto.nOrdenCompra);
                if (orden == null) throw new Exception($"orden de compra {dto.nOrdenCompra} no encontrada ");

                var articulo = await _articuloRepository.GetByIdAsync(dto.idArticulo);
                if (articulo == null) throw new Exception($"articulo con Id {dto.idArticulo} no encontrado ");

                var proveedorArt = await _proveedorArticuloRepository.GetProvArtByIdsAsync(dto.idArticulo, orden.proveedor.idProveedor);
                if (proveedorArt == null)
                    throw new Exception($"el proveedor de la orden no esta asociado al articulo Id {dto.idArticulo} ");

                long cantidad=0;
                string? advertencia = null;

                if (articulo.modeloInv == ModeloInv.LoteFijo_Q)
                {
                    cantidad = articulo.qOptimo;
                }
                else if (articulo.modeloInv == ModeloInv.PeriodoFijo_P)
                {
                    cantidad = await _proveedorArticuloService.CalcCantidadAPedirP(articulo, proveedorArt);
                    if (cantidad == 0)
                    {
                        advertencia = $"el calculo automatico de cantidad a pedir para el articulo '{articulo.nombreArticulo}' (ID {articulo.idArticulo}) resulta en valores fuera de rango, debe ingresar la cantidad manualmente ";
                        cantidad = 1;
                    }
                } else {throw new Exception("modelo de inventario no soportado para el articulo ");}

                double precioUnitario = proveedorArt.precioUnitario;
                double subtotal = cantidad * precioUnitario;

                var detalle = new DetalleOrdenCompra
                {
                    articulo = articulo,
                    ordenCompra = orden,
                    cantidadArticulos = cantidad,
                    precioSubTotal = subtotal
                };

                await _detalleOrdenCompraRepository.AddAsync(detalle);
                await ActualizarTotalOrdenCompra(dto.nOrdenCompra);

                return (advertencia, detalle);
            }

        public async Task ModDetalleOC(OrdenCompraDetalleABMdto dto)
        {
            var orden = await _ordenCompraRepository.GetOrdenCompraConEstado(dto.nOrdenCompra);
            if (orden == null) throw new Exception($"orden de compra {dto.nOrdenCompra} no encontrada");

            var detalle = await _detalleOrdenCompraRepository.GetDetalleByOrdenYArticulo(dto.nOrdenCompra,dto.idArticulo);
            if (detalle == null) throw new Exception($"detalle no encontrado para articulo {dto.idArticulo} en orden {dto.nOrdenCompra}");

            var proveedorArt = await _proveedorArticuloRepository.GetProvArtByIdsAsync(dto.idArticulo, orden.proveedor.idProveedor);
            if (proveedorArt == null) throw new Exception($"proveedor no asociado al articulo {dto.idArticulo}");

            detalle.cantidadArticulos = dto.cantidad;
            detalle.precioSubTotal = dto.cantidad * proveedorArt.precioUnitario;

            await _detalleOrdenCompraRepository.UpdateAsync(detalle);
            await ActualizarTotalOrdenCompra(dto.nOrdenCompra);
        }

        public async Task DeleteDetalleOC(OrdenCompraDetalleABMdto dto)
        {
            var detalle = await _detalleOrdenCompraRepository.GetDetalleByOrdenYArticulo(dto.nOrdenCompra,dto.idArticulo );
            if (detalle == null) throw new Exception($"detalle no encontrado para articulo {dto.idArticulo} en orden {dto.nOrdenCompra}");

            await _detalleOrdenCompraRepository.DeleteAsync(detalle);
            await ActualizarTotalOrdenCompra(dto.nOrdenCompra);
        }

        private async Task ActualizarTotalOrdenCompra(long nOrdenCompra)
        {
            var detalles = await _detalleOrdenCompraRepository.GetDetallesByOrdenId(nOrdenCompra);
            var orden = await _ordenCompraRepository.GetOrdenCompraConEstado(nOrdenCompra);
            if (orden == null) throw new Exception($"rrden de compra {nOrdenCompra} no encontrada ");

            double nuevoTotal = detalles.Sum(d => d.precioSubTotal);
            orden.totalPagar = Math.Round(nuevoTotal, 3);
            await _ordenCompraRepository.UpdateAsync(orden);
        }
    }
}
