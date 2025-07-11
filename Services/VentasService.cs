using ISession = NHibernate.ISession;
using Proyect_InvOperativa.Dtos.Ventas;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Models.Enums;
using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Dtos.Articulo;

namespace Proyect_InvOperativa.Services
{
    public class VentasService
    {


        private readonly StockArticuloRepository _stockArticuloRepository;
        private readonly ArticuloRepository _articuloRepository;
        private readonly DescuentoArticuloRepository _descuentoarticuloRepository;        
        private readonly ProveedorArticuloRepository _proveedorArticuloRepository;
        private readonly BaseRepository<DetalleVentas> _detalleVentasRepository;
        private readonly VentasRepository _ventasRepository;
        private readonly OrdenCompraRepository _oCompraRepository;
        private readonly OrdenCompraService _oCompraService;
        private readonly ISession _session;


        public VentasService(StockArticuloRepository stockArticuloRepository,DescuentoArticuloRepository descuentoArticuloRepository,OrdenCompraService oCompraService,OrdenCompraRepository oCompraRepository,ProveedorArticuloRepository proveedorArticuloRepository, ArticuloRepository articuloRepository, BaseRepository<DetalleVentas> detalleVentasRepository, VentasRepository ventasRepository, ISession session)
        {
            _stockArticuloRepository = stockArticuloRepository;
            _articuloRepository = articuloRepository;
            _descuentoarticuloRepository = descuentoArticuloRepository;
            _oCompraRepository = oCompraRepository;
            _oCompraService = oCompraService;
            _proveedorArticuloRepository =proveedorArticuloRepository;
            _detalleVentasRepository = detalleVentasRepository;
            _ventasRepository = ventasRepository;
            _session = session;
        }
        
        #region Actualizar stock (ventas)
        public async Task<bool> ValidarStockDisponible(StockDto stockDto)
        {
            long idArticulo = stockDto.idArticulo;
            long cantidadSolicitada = stockDto.cantidad;
            var stockArticulo = await _stockArticuloRepository.getstockActualbyIdArticulo(idArticulo);

            if (stockArticulo == null) return false; 
            return (stockArticulo.stockActual >= cantidadSolicitada);
        }

        private async Task<string?> ActualizarStockVenta(Articulo articulo, DetalleVentas detalle)
        {

            var stockArticulo = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
            if (stockArticulo == null) throw new Exception($"no se encuentra stock actual para el articulo con Id {articulo.idArticulo} ");
            
            // actualiza el stock restando la cantidad vendida
            stockArticulo.stockActual -= detalle.cantidad;

            // actualiza control si el stock queda por debajo del stock de seguridad
            if (stockArticulo.stockActual <= stockArticulo.stockSeguridad)
            {
                stockArticulo.control = true;
            }

            // advertencia si es modelo Q y se alcanza el punto de pedido
            string? aviso_pp = null;
            if (articulo.modeloInv == ModeloInv.LoteFijo_Q)
            {
                if (stockArticulo.stockActual < stockArticulo.puntoPedido)
                {
                    aviso_pp = $"el articulo '{articulo.nombreArticulo}' alcanzo o esta por debajo del punto de pedido ";
                    var proveedoresArticulo = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
                    if (!proveedoresArticulo.Any()) throw new Exception($"no se encuentran proveedores para el articulo con Id {articulo.idArticulo} ");

                    //  selecciona proveedor predeterminado
                    var proveedorArt = proveedoresArticulo.FirstOrDefault(pPred => pPred.predeterminado);
                    if (proveedorArt == null) throw new Exception($"no se encuentra proveedor predeterminado para el articulo con Id {articulo.idArticulo} ");;
                    long idProv = proveedorArt.proveedor!.idProveedor;

                    var ordenesVigentesArt = await _oCompraRepository.GetOrdenesVigentesArt(articulo.idArticulo, new[] { "Pendiente", "Enviada" });
                    if (!ordenesVigentesArt.Any())
                    {
                        var articuloDto = new ArticuloDto {
                        idArticulo = articulo.idArticulo
                        };
                        await _oCompraService.GenerarOrdenCompra(new List<ArticuloDto> {articuloDto}, idProv);
                    }
                }
            }
             await _stockArticuloRepository.UpdateAsync(stockArticulo);
            await _session.UpdateAsync(stockArticulo);
            return aviso_pp;
        }
        #endregion

        #region create ventas
        public async Task<VentaResultadoDto> CreateVentas(VentasDto ventasDto)
        {
            if (ventasDto.detalles == null || ventasDto.detalles.Length < 1)  throw new Exception("no se dispone de articulos en la venta recibida ");

            var resultadoVenta = new VentaResultadoDto();
            var advertencias_Stock = new List<string>();
            var advertencias_Descuentos = new List<string>();
            double totalVenta = 0;

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    foreach (var detalle in ventasDto.detalles)
                    {
                        var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(detalle.idArticulo);
                        if (stock == null || stock.fechaStockFin != null) throw new Exception($"el articulo con Id {detalle.idArticulo} no tiene stock activo ");

                        if (detalle.cantidadArticulo > stock.stockActual) throw new Exception($"la cantidad solicitada para el articulo con Id {detalle.idArticulo} supera el stock disponible ({stock.stockActual}).");
                    }

                    var venta = new Ventas
                    {
                        descripcionVenta = ventasDto.descripcionVenta,
                        totalVenta = 0.0
                    };
                    await _ventasRepository.AddAsync(venta);

                    foreach (var detalle in ventasDto.detalles)
                    {
                        var articulo = await _articuloRepository.GetByIdAsync(detalle.idArticulo);
                        if (articulo is null)  throw new Exception($"no se encuentra el articulo con Id: {detalle.idArticulo}");
                        if (articulo.precioVenta <= 0.0) throw new Exception($"el articulo '{articulo.nombreArticulo}' no tiene un precio de venta valido ");

                        var descuento = await _descuentoarticuloRepository.getdescActualbyIdArticulo(detalle.idArticulo);
                        double precioAplicado = articulo.precioVenta;

                        if (descuento != null && detalle.cantidadArticulo >= descuento.cantidadDesc)
                        {
                            var porcentaje = descuento.porcentajeDesc;
                            precioAplicado = articulo.precioVenta * (1 - porcentaje / 100.0);
                            advertencias_Descuentos.Add($"descuento aplicado del {porcentaje} % por cantidad al articulo '{articulo.nombreArticulo}' ");
                        }
                        double subtotal = Math.Round(detalle.cantidadArticulo * precioAplicado, 4);

                        var newDetalle = new DetalleVentas
                        {
                            cantidad = detalle.cantidadArticulo,
                            subTotalVenta = subtotal,
                            articulo = articulo,
                            venta = venta
                        };
                        totalVenta += subtotal;
                        var aviso_stock = await ActualizarStockVenta(articulo, newDetalle);
                        if (!string.IsNullOrEmpty(aviso_stock)) advertencias_Stock.Add(aviso_stock);
                        await _detalleVentasRepository.AddAsync(newDetalle);
                    }

                    venta.totalVenta = Math.Round(totalVenta, 4);
                    venta.fechaVenta = DateTime.UtcNow;
                    await _ventasRepository.UpdateAsync(venta);
                    await tx.CommitAsync();

                    resultadoVenta.venta = venta;
                    resultadoVenta.advertencias = advertencias_Stock.Concat(advertencias_Descuentos).ToList();
                    return resultadoVenta;
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
        }
        #endregion

        #region lista ventas por articulo
        public async Task<List<ArtVentasDto>> GetVentasPorArticulo(long idArticulo)
            {
                var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(idArticulo);
                if (stock == null || stock.fechaStockFin != null)
                {
                    throw new Exception($"el articulo con Id {idArticulo} no existe o fue dado de baja ");
                }

                // obtener ventas relacionadas
                var ventasDetalle = await _ventasRepository.GetVentasDetallePorArticulo(idArticulo);

                return ventasDetalle.Select(vdet => new ArtVentasDto
                {
                    nVenta = vdet.venta!.nVenta,
                    fechaVenta = vdet.venta!.fechaVenta,
                    cantidadVendida = vdet.cantidad,
                    subtotal = vdet.subTotalVenta
                }).ToList();
            }
        #endregion
        
        #region mostrar detalles de venta a registrar
        public async Task<(VentasDto ventaDto, VentaResultadoDto resultado)> MostrarDetallesDeVentaARegistrar(VentasDto ventaDto)
        {
            double totalVenta = 0;
            var articulosSinStock = new List<long>();
            var advertenciasDescuentos = new List<string>();

            foreach (var detalle in ventaDto.detalles)
            {
                var articulo = await _articuloRepository.GetByIdAsync(detalle.idArticulo);
                if (articulo == null) throw new Exception($"articulo con Id {detalle.idArticulo} no encontrado.");

                var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                if (stock == null || detalle.cantidadArticulo > stock.stockActual)
                {
                    articulosSinStock.Add(articulo.idArticulo);
                    continue;
                }
                double precioVenta = articulo.precioVenta;
                double precioConDescuento = precioVenta;

                var descuento = await _descuentoarticuloRepository.getdescActualbyIdArticulo(articulo.idArticulo);
                if (descuento != null && detalle.cantidadArticulo >= descuento.cantidadDesc)
                {
                    precioConDescuento = precioVenta * (1 - descuento.porcentajeDesc / 100.0);
                    advertenciasDescuentos.Add($"se aplicara un descuento del {descuento.porcentajeDesc}% sobre el articulo '{articulo.nombreArticulo}': precio sin descuento = $ {precioVenta:F2}, precio con descuento = $ {precioConDescuento:F2}");
                }
                double subtotal = detalle.cantidadArticulo * precioConDescuento;
                detalle.subtotalVenta = Math.Round(subtotal, 4);
                totalVenta += subtotal;
            }

            if (articulosSinStock.Any())
            {
                string ids = string.Join(", ", articulosSinStock);
                throw new Exception($"las cantidades solicitadas para el/los articulo/s con Id: {ids} superan el stock actual ");
            }

            ventaDto.totalVenta = Math.Round(totalVenta, 3);
            var resultado = new VentaResultadoDto
            {
                venta = null, 
                advertencias = advertenciasDescuentos
            };
            return (ventaDto, resultado);
        }
        #endregion

        #region precio de venta de articulo
        public async Task<double> GetPrecioVentaArticulo(long idArticulo)
        {
            var articulo = await _articuloRepository.GetByIdAsync(idArticulo);
            if (articulo == null) throw new Exception($"articulo con Id {idArticulo} no encontrado ");

            var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(idArticulo);
            if (stock == null || stock.fechaStockFin != null) throw new Exception($"el articulo con Id {idArticulo} esta dado de baja ");
            return articulo.precioVenta;
        }


        public async Task ActualizarPrecioVentaArt(PrecioVentaArtDto dto)
        {
            var articulo = await _articuloRepository.GetByIdAsync(dto.idArticulo);
            if (articulo == null) throw new Exception($"articulo con Id {dto.idArticulo} no encontrado ");

            var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(dto.idArticulo);
            if (stock == null || stock.fechaStockFin != null) throw new Exception($"el artículo con Id {dto.idArticulo} esta dado de baja ");

            if (dto.precioVentaArt == null || dto.precioVentaArt <= 0) throw new Exception("el precio de venta debe ser un valor numerico mayor a 0 ");
            articulo.precioVenta = dto.precioVentaArt.Value;
            await _articuloRepository.UpdateAsync(articulo);
        }
        #endregion
    }
}