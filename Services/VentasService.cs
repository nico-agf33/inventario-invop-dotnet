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
        #region LISTA DE FALTANTES
        //Alta Venta
        #endregion

        private readonly StockArticuloRepository _stockArticuloRepository;
        private readonly ArticuloRepository _articuloRepository;
        private readonly ProveedorArticuloRepository _proveedorArticuloRepository;
        private readonly BaseRepository<DetalleVentas> _detalleVentasRepository;
        private readonly VentasRepository _ventasRepository;
        private readonly OrdenCompraRepository _oCompraRepository;
        private readonly OrdenCompraService _oCompraService;
        private readonly ISession _session;


        public VentasService(StockArticuloRepository stockArticuloRepository,OrdenCompraService oCompraService,OrdenCompraRepository oCompraRepository,ProveedorArticuloRepository proveedorArticuloRepository, ArticuloRepository articuloRepository, BaseRepository<DetalleVentas> detalleVentasRepository, VentasRepository ventasRepository, ISession session)
        {
            _stockArticuloRepository = stockArticuloRepository;
            _articuloRepository = articuloRepository;
            _oCompraRepository = oCompraRepository;
            _oCompraService = oCompraService;
            _proveedorArticuloRepository =proveedorArticuloRepository;
            _detalleVentasRepository = detalleVentasRepository;
            _ventasRepository = ventasRepository;
            _session = session;
        }
        
        #region Actualizar stock (ventas)
        public async Task<bool> ValidarStockDisponible(StockDto ventasDto)
        {
            long idArticulo = ventasDto.idArticulo;
            long cantidadSolicitada = ventasDto.cantidad;
            var stockArticulo = await _stockArticuloRepository.getstockActualbyIdArticulo(idArticulo);

            if (stockArticulo == null) return false; 
            return stockArticulo.stockActual >= cantidadSolicitada;
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
                if (ventasDto.detalles.Length<1)  throw new Exception("no se dispone de articulos en la venta recibida ");
                    var resultadoVenta = new VentaResultadoDto();
                    var advertencias_Stock = new List<string>();
                using (var tx = _session.BeginTransaction())
                {
                    try
                    {
                        var venta = new Ventas
                        {
                            descripcionVenta = ventasDto.descripcionVenta,
                            //totalVenta = 0
                            totalVenta = ventasDto.totalVenta ?? 0 
                        };

                        await _ventasRepository.AddAsync(venta); 
                        //double total = 0;

                        foreach (var detalle in ventasDto.detalles)
                        {
                            var articulo = await _articuloRepository.GetByIdAsync(detalle.idArticulo);
                            if (articulo is null) throw new Exception($"no se encuentra el articulo con Id: {detalle.idArticulo} ");

                            //var proveedoresRelacionados = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
                            //var proveedorPredeterminado = proveedoresRelacionados.FirstOrDefault(pArt => pArt.predeterminado);
                            //if (proveedorPredeterminado == null) throw new Exception($"no hay proveedor predeterminado para el articulo {articulo.nombreArticulo}");

                           //var precioCompra = proveedorPredeterminado.precioUnitario;
                            //var subtotal = detalle.cantidadArticulo * precioCompra*1.15;

                            var newDetalle = new DetalleVentas
                            {
                                cantidad = detalle.cantidadArticulo,
                                subTotalVenta = detalle.subtotalVenta ?? 0, 
                                articulo = articulo,
                                venta = venta
                            };
                            //total += subtotal;

                            var aviso_stock = await ActualizarStockVenta(articulo, newDetalle);
                            if (!string.IsNullOrEmpty(aviso_stock)) advertencias_Stock.Add(aviso_stock);
                            await _detalleVentasRepository.AddAsync(newDetalle); 
                        }

                        //venta.totalVenta = total;
                        await _ventasRepository.UpdateAsync(venta);
                        await tx.CommitAsync();
                        resultadoVenta.venta = venta;
                        resultadoVenta.advertencias = advertencias_Stock;
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
                    cantidadVendida = vdet.cantidad,
                    subtotal = vdet.subTotalVenta
                }).ToList();
            }
        #endregion
        
        #region mostrar detalles de venta a registrar
            public async Task<VentasDto> MostrarDetallesDeVentaARegistrar(VentasDto ventaDto)
            {
                double totalVenta = 0;
                var articulosSinStock = new List<long>();

                foreach (var detalle in ventaDto.detalles)
                {
                    var articulo = await _articuloRepository.GetByIdAsync(detalle.idArticulo);
                    if (articulo == null) throw new Exception($"articulo con Id {detalle.idArticulo} no encontrado.");

                    var proveedoresRelacionados = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
                    var proveedorPredeterminado = proveedoresRelacionados.FirstOrDefault(pArt => pArt.predeterminado);
                    if (proveedorPredeterminado == null) throw new Exception($"no hay proveedor predeterminado para el articulo {articulo.nombreArticulo}");

                    var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                    if (stock == null || detalle.cantidadArticulo > stock.stockActual)
                    {
                        articulosSinStock.Add(articulo.idArticulo);
                        continue;
                    }
                    double precioCompra = proveedorPredeterminado.precioUnitario;
                    double subtotal = detalle.cantidadArticulo * precioCompra * 1.15;
                    detalle.subtotalVenta = Math.Round(subtotal, 3);
                    totalVenta += subtotal;
                }

                if (articulosSinStock.Any())
                {
                    string ids = string.Join(", ", articulosSinStock);
                    throw new Exception($"Las cantidades solicitadas para el/los artículo/s {ids} superan el stock actual.");
                }
                ventaDto.totalVenta = Math.Round(totalVenta, 3);
                return ventaDto;
            }
        #endregion
    }
}