using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Utils;
using Proyect_InvOperativa.Repository;


namespace Proyect_InvOperativa.Services
{
    public class ProveedorArticuloService
    {
        
        private readonly ProveedoresRepository _proveedoresRepository;
        private readonly ArticuloRepository _articuloRepository;
        private readonly ProveedorArticuloRepository _proveedoresArticuloRepository;
        private readonly StockArticuloRepository _stockArtRepository;
        private readonly OrdenCompraRepository _oCompraRepository;
        private readonly DetalleOrdenCompraRepository _detOrdCRepository;

        public ProveedorArticuloService(ProveedoresRepository proveedoresRepository,DetalleOrdenCompraRepository detOrdCRepository,OrdenCompraRepository oCompraRepository,ArticuloRepository artRepo, ProveedorArticuloRepository  pArtRepo,StockArticuloRepository stockArticuloRepository) 
        {
            _proveedoresRepository = proveedoresRepository;
            _articuloRepository = artRepo;  
            _detOrdCRepository = _detOrdCRepository;
            _oCompraRepository = oCompraRepository;
            _proveedoresArticuloRepository = pArtRepo;
            _stockArtRepository = stockArticuloRepository;
        }
        public async Task<ProveedorArticulo> CreateProveedorArticulo(ProveedorArticuloDto provArtDto)
        {
            var articulo = await _articuloRepository.GetByIdAsync(provArtDto.idArticulo);
            var proveedor = await _proveedoresRepository.GetByIdAsync(provArtDto.idProveedor);

            var proveedorArticulo = new ProveedorArticulo()
            {
                precioUnitario = provArtDto.precioUnitario,
                costoPedido = provArtDto.costoPedido,
                tiempoEntregaDias = provArtDto.tiempoEntregaDias,
                fechaFinProveedorArticulo = null,
                proveedor = proveedor,
                articulo = articulo
            };
            var listaANew = await _proveedoresArticuloRepository.AddAsync(proveedorArticulo);
            return proveedorArticulo;
        }

        public async Task DeleteProveedorArticulo(ProveedorArticuloDto ProvArtDto)
        {
            var proveedorArticulo = await _proveedoresArticuloRepository.GetProvArtByIdsAsync(ProvArtDto.idArticulo, ProvArtDto.idProveedor);
            if (proveedorArticulo == null){throw new Exception("error: proveedor y articulo no estan relacionados");}

            // validar proveedor predeterminado
            if (proveedorArticulo.predeterminado){throw new Exception("el proveedor es el predeterminado para este artículo, no se puede dar de baja ");}

                proveedorArticulo.fechaFinProveedorArticulo = DateTime.UtcNow;
                await _proveedoresArticuloRepository.UpdateAsync(proveedorArticulo);
        }


        #region calcular cant. a pedir 
        public async Task<long> CalcCantidadAPedirP(Articulo articulo, ProveedorArticulo proveedorArt)
        {
            var unidad = articulo.unidadTemp;
            double T = ModInventarioUtils.ConvertirDesdeDias(articulo.tiempoRevisionDias, unidad);
            double L = ModInventarioUtils.ConvertirDesdeDias(proveedorArt.tiempoEntregaDias, unidad);
            double periodoVulnerable = T + L;
            double valSigma = ModInventarioUtils.ConvertirDesvEstandarDesdeAnual(articulo.desviacionEstandarDemanda, unidad);
            var Z = ModInventarioUtils.ObtenerZ(articulo.nivelServicio);
            double dEst = articulo.demandaEst;

            var stock = await _stockArtRepository.getstockActualbyIdArticulo(articulo.idArticulo);
            if (stock == null) return 0;

            // verificacion con unidades solicitadas en ordenes de compra
            var ordenesVigentesArt = await _oCompraRepository.GetOrdenesVigentesArt(articulo.idArticulo, new[] { "Enviada" });
            long stockPedido = 0;
            foreach (var orden in ordenesVigentesArt)
            {
                var detallesOrden = await _detOrdCRepository.GetDetallesByOrdenId(orden.nOrdenCompra);
                foreach (var detalle in detallesOrden)
                {
                    if (detalle.articulo.idArticulo == articulo.idArticulo)
                    {
                        stockPedido += detalle.cantidadArticulos;
                    }
                }
            }

            // stock de seguridad
            double stockSeguridad = Z * valSigma * Math.Sqrt(periodoVulnerable);
            long stockSeguridadEnt = (long)Math.Ceiling(stockSeguridad);

            // calcula cantidad a pedir
            double q = dEst * periodoVulnerable + stockSeguridad - (stock.stockActual + stockPedido);
            long qEnt = (long)Math.Ceiling(q);
            if (qEnt < 0) qEnt = 0;

            // actualiza stock
            stock.stockSeguridad = stockSeguridadEnt;
            await _stockArtRepository.UpdateAsync(stock);

            return qEnt;
        }
        #endregion

        #region ACTUALIZAR ProveedorArticulo
        public async Task UpdateProveedorArticulo(ProveedorArticuloDto paDto)
        {
            var proveedorArticulo = await _proveedoresArticuloRepository.GetProvArtByIdsAsync(paDto.idArticulo,paDto.idProveedor);
            if (proveedorArticulo is null)
            {
                throw new Exception($"proveedorArticulo con Id: {paDto.idProveedor} no encontrado ");
            }
            // validar que el proveedor exista
            var proveedor = await _proveedoresRepository.GetProveedorById(proveedorArticulo.proveedor!.idProveedor);
            if (proveedor == null)
            {
                throw new Exception($"proveedor con Id: {proveedorArticulo.proveedor.idProveedor} no encontrado ");
            }
            // validar que el articulo exista
            var articulo = await _articuloRepository.GetArticuloById(paDto.idArticulo);
            if (articulo == null)
            {
                throw new Exception($"articulo con Id: {paDto.idArticulo} no encontrado ");
            }

            proveedorArticulo.precioUnitario = paDto.precioUnitario;
            proveedorArticulo.tiempoEntregaDias = paDto.tiempoEntregaDias;
            proveedorArticulo.costoPedido = paDto.costoPedido;
            await _proveedoresArticuloRepository.UpdateAsync(proveedorArticulo);
        }
        #endregion

        #region lista articulos NO relacionados al proveedor
        public async Task<List<ArticuloDto>> ArticulosNoRelacionadosProv(long idProveedor)
        {
            var articulos = await _articuloRepository.GetAllArticulos();
            var articulosRelacionados = await _proveedoresArticuloRepository.GetAllByProveedorIdAsync(idProveedor);

            var idsRelacionados = articulosRelacionados
            .Select(pArt => pArt.articulo.idArticulo)
            .ToHashSet();

            var articulosNoRelacionados = new List<ArticuloDto>();

            foreach (var art in articulos)
            {
                if (idsRelacionados.Contains(art.idArticulo)) continue;
                var stock = await _stockArtRepository.getstockActualbyIdArticulo(art.idArticulo);
                if (stock == null || stock.fechaStockFin != null) continue;

                articulosNoRelacionados.Add(new ArticuloDto
                {
                    idArticulo = art.idArticulo,
                    nombreArticulo = art.nombreArticulo
                });
            }
            return articulosNoRelacionados;
        }
        #endregion
    }
}
