using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Dtos.MaestroArticulo;
using Proyect_InvOperativa.Dtos.Proveedor;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Models.Enums;
using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Utils;

namespace Proyect_InvOperativa.Services
{
    #region LISTA DE FALTANTES
    /// <summary>
    //Creacion, modificacion y eliminacion de Articulo (LISTO)
    //    Asignar proveedor Predeterminado
    //    Determinacion del modelo de inventario
    //    Calculo del modelo lote fijo
    //    Calculo del modelo Intervalo fijo
    //    Calculo del CGI
    //    Listado de productos a reponer //// Para hacer el pedido
    //    Listado Productos Faltantes
    //    Listado Proveedores x Articulo
    /// </summary>
    #endregion
    public class MaestroArticulosService
    {
        private readonly ArticuloRepository _articuloRepository;
        private readonly ProveedoresRepository _proveedorRepository;
        private readonly OrdenCompraRepository _ordenCompraRepository;
        private readonly OrdenCompraService _ordenCompraService;
        private readonly EstadoProveedoresRepository _estProveedorRepository;
        private readonly OrdenCompraEstadoRepository _ordenCompraEstadoRepository;
        private readonly MaestroArticulosRepository _maestroArticuloRepository;
        private readonly StockArticuloRepository _stockArticuloRepository;
        private readonly ProveedorArticuloRepository _proveedorArticuloRepository;
        private readonly ProveedorArticuloService _proveedorArtService;

        public MaestroArticulosService(ArticuloRepository articuloRepository,EstadoProveedoresRepository estProveedorRepository, ProveedoresRepository proveedorRepository, OrdenCompraRepository ordenCompraRepository, OrdenCompraService ordenCompraService, OrdenCompraEstadoRepository ordenCompraEstadoRepository, MaestroArticulosRepository maestroArticulosRepository, StockArticuloRepository stockRepo, ProveedorArticuloRepository PARepository, ProveedorArticuloService proveedorArticuloService)
        {
            _articuloRepository = articuloRepository;
            _proveedorRepository = proveedorRepository;
            _estProveedorRepository = estProveedorRepository;
            _ordenCompraRepository = ordenCompraRepository;
            _ordenCompraService = ordenCompraService;
            _ordenCompraEstadoRepository = ordenCompraEstadoRepository;
            _maestroArticuloRepository = maestroArticulosRepository;
            _stockArticuloRepository = stockRepo;
            _proveedorArticuloRepository = PARepository;
            _proveedorArtService = proveedorArticuloService;
        }
        #region AB Maestro Articulo
        public async Task<MaestroArticulo> CreateMaestroArticulo(CreateMaestroArticuloDto createMaestroArticuloDto)
        {
            var maestro = new MaestroArticulo()
            {
                idMaestroArticulo = 1,
                nombreMaestro = createMaestroArticuloDto.nombreMaestroArticulo,
            };

            var newMaestro = await _maestroArticuloRepository.AddAsync(maestro);

            return newMaestro;

        }

        public async Task DeleteMaestroArticulo(long idMaestroArticulo)
        {
            var maestroArticulo = await _maestroArticuloRepository.GetByIdAsync(idMaestroArticulo);

            if (maestroArticulo is null)
            {
                throw new Exception($"Artículo con id: {idMaestroArticulo} no encontrado. ");
            }

            await _maestroArticuloRepository.DeleteIdAsync(idMaestroArticulo);
        }
        #endregion

        #region ABM Articulo


        public async Task<Articulo> CreateArticulo(ArticuloDto ArticuloDto)
        {
            var maestro = await _maestroArticuloRepository.GetByIdAsync(1); //debe haber otra forma, es para que funcione, despues lo arreglo 

            var articulo = new Articulo()
            {

                nombreArticulo = ArticuloDto.nombreArticulo,
                descripcion = ArticuloDto.descripcion,
                demandaEst = ArticuloDto.demandaEst,
                costoAlmacen = ArticuloDto.costoAlmacen,
                tiempoRevisionDias = ArticuloDto.tiempoRevisionDias,
                modeloInv = (ModeloInv)ArticuloDto.modeloInv,
                unidadTemp = (UnidadTemp)ArticuloDto.unidadTemp,
                fechaRevisionP = DateTime.Now,
                masterArticulo = maestro,
                nivelServicio = ArticuloDto.nivelServicio,
                desviacionEstandarDemanda = ArticuloDto.desviacionEstandarDemanda,
                stockMax = ArticuloDto.stockMax
            };

            var newArticulo = await _articuloRepository.AddAsync(articulo);

            var articuloStock = new StockArticulos()
            {
                stockSeguridad = 0,
                stockActual = 0,
                fechaStockInicio = DateTime.UtcNow,
                fechaStockFin = null,
                articulo = newArticulo // importante: referencia al artículo persistido
            };

            var newArticuloStock = await _stockArticuloRepository.AddAsync(articuloStock);

            return newArticulo;
        }


        public async Task UpdateArticulo(ArticuloDto ArticuloDto)
        {
            var articuloModificado = await _articuloRepository.GetArticuloById(ArticuloDto.idArticulo);
            //var stockAsociadoArticulo = await _stockArticuloRepository.getstockActualbyIdArticulo(ArticuloDto.idArticulo); //falta testear

            if (articuloModificado is null)
            {
                throw new Exception($"articulo con Id: {ArticuloDto.idArticulo} no encontrado  ");
            }
            // MODIFICAR LOS DATOS PROPIOS DE ARTICULO
            articuloModificado.nombreArticulo = ArticuloDto.nombreArticulo;
            articuloModificado.descripcion = ArticuloDto.descripcion;
            articuloModificado.demandaEst = ArticuloDto.demandaEst;
            articuloModificado.costoAlmacen = ArticuloDto.costoAlmacen;
            articuloModificado.tiempoRevisionDias = ArticuloDto.tiempoRevisionDias;
            articuloModificado.unidadTemp = (UnidadTemp)ArticuloDto.unidadTemp;
            articuloModificado.modeloInv = (ModeloInv)ArticuloDto.modeloInv;
            articuloModificado.nivelServicio = ArticuloDto.nivelServicio;
            articuloModificado.desviacionEstandarDemanda = ArticuloDto.desviacionEstandarDemanda;
            articuloModificado.stockMax = ArticuloDto.stockMax;
            // MODIFICAR LOS DATOS PROPIOS DE STOCK ASOCIADO A ARTICULO, si es que se pueden 

            await _articuloRepository.UpdateAsync(articuloModificado);
            //await _stockArticuloRepository.UpdateAsync(stockAsociadoArticulo!);

        }

        public async Task DeleteArticulo(long idArticulo)
        {

            var artEliminar = await _articuloRepository.GetArticuloById(idArticulo);

            if (artEliminar is null)
            {
                throw new Exception($"articulo con Id: {idArticulo} no encontrado  ");
            }

            var ordenesVigentesArt = await _ordenCompraRepository.GetOrdenesVigentesArt(idArticulo, new[] { "Pendiente", "Enviada" });

            if (ordenesVigentesArt.Any())
            {
                throw new Exception($"no se puede eliminar el articulo con Id: {idArticulo} porque tiene ordenes de compra pendientes o enviadas ");
            }

            var stockAsociado = await _stockArticuloRepository.getstockActualbyIdArticulo(idArticulo);
            if (stockAsociado is null)
            {
                throw new Exception($"no se encuentra stock asociado al IdArticulo: {idArticulo} ");
            }

            if (stockAsociado.stockActual>0)
            {
                throw new Exception($"no se puede eliminar el articulo con Id: {idArticulo} porque aun tiene unidades en stock ");

            }

            stockAsociado.fechaStockFin = DateTime.UtcNow;
            await _stockArticuloRepository.UpdateAsync(stockAsociado);

                var relacionesProvArt = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(idArticulo);
                foreach (var relacionArtProv in relacionesProvArt)
                {
                    relacionArtProv.fechaFinProveedorArticulo = DateTime.UtcNow;
                    await _proveedorArticuloRepository.UpdateAsync(relacionArtProv);
                }
        }

        public async Task<IEnumerable<Articulo>> GetAllArticulos()
        {
            var articulos = await _articuloRepository.GetAllArticulos();

            return articulos;
        }

        public async Task<Articulo?> GetArticuloById(long idArticulo)
        {
            var articulo = await _articuloRepository.GetArticuloById(idArticulo);

            return articulo;
        }

        public List<object> GetModelosInventario()
        {
            return Enum.GetValues(typeof(ModeloInv))
            .Cast<ModeloInv>()
            .Select(eModInv => new { id = (int)eModInv, nombreModInv = eModInv.ToString() })
            .ToList<object>();
        }

        public List<object> GetUnidadesTemp()
        {
            return Enum.GetValues(typeof(UnidadTemp))
            .Cast<UnidadTemp>()
            .Select(uTemp => new { id = (int)uTemp, nombreUnidadTemp = uTemp.ToString() })
            .ToList<object>();
        }

        #region Calculo mod. de inventario
            public async Task<List<ArticuloInvDto>> CalculoModInv()
            {
                var articulos = await _articuloRepository.GetAllArticulos();
                var listaArt = new List<ArticuloInvDto>();
                foreach (var articulo in articulos)
                {
                    // obtener stock asociado
                    var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                    if (stock == null || stock.fechaStockFin != null)
                    {
                        continue; // articulo dado de baja
                    }

                    // verificar modelo de inventario 
                    switch (articulo.modeloInv)
                    {
                        case ModeloInv.LoteFijo_Q:
                            await CalculoLoteFijoQ(articulo, stock);
                            break;
                        case ModeloInv.PeriodoFijo_P:
                            await CalculoPeriodoFijoP(articulo, stock);
                            break;
                        default:
                            // control redundante
                        continue;
                    }
                            // proveedor predeterminado
                        var proveedoresArt = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
                        if (!proveedoresArt.Any()) continue;

                        var proveedorPred = proveedoresArt.FirstOrDefault(pred => pred.predeterminado)?.proveedor?.nombreProveedor ?? "";

                    listaArt.Add(new ArticuloInvDto
                    {
                        idArticulo = articulo.idArticulo,
                        nombreArticulo = articulo.nombreArticulo,
                        descripcion = articulo.descripcion,
                        modeloInv = articulo.modeloInv.ToString(),
                        demandaEst = articulo.demandaEst,
                        tiempoRevisionDias = articulo.tiempoRevisionDias,
                        costoAlmacen = articulo.costoAlmacen,
                        unidadTemp = articulo.unidadTemp.ToString(),
                        proveedor = proveedorPred,
                        stockActual = stock.stockActual,
                        stockSeguridad = stock.stockSeguridad,
                        puntoPedido = stock.puntoPedido,
                        cgi = Math.Round(articulo.cgi,4),
                        stockMax = articulo.stockMax
                    });
                }
                return listaArt;
            }
        #endregion

        #region Lista Articulos y datos
            public async Task<List<ArticuloInvDto>> ListarArticulosYDatos()
            {
                var articulos = await _articuloRepository.GetAllArticulos();
                var listaArtD = new List<ArticuloInvDto>();
                foreach (var articulo in articulos)
                {
                    // obtener stock asociado
                    var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                    if (stock == null || stock.fechaStockFin != null) {continue; }

                            // buscar proveedor predeterminado
                        var proveedoresArt = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
                        // if (!proveedoresArt.Any()) continue;

                        var proveedorPred = proveedoresArt.FirstOrDefault(pred => pred.predeterminado)?.proveedor?.nombreProveedor ?? "";

                    listaArtD.Add(new ArticuloInvDto
                    {
                        idArticulo = articulo.idArticulo,
                        nombreArticulo = articulo.nombreArticulo,
                        descripcion = articulo.descripcion,
                        modeloInv = articulo.modeloInv.ToString(),
                        demandaEst = articulo.demandaEst,
                        tiempoRevisionDias = articulo.tiempoRevisionDias,
                        costoAlmacen = articulo.costoAlmacen,
                        unidadTemp = articulo.unidadTemp.ToString(),
                        proveedor = proveedorPred,
                        stockActual = stock.stockActual,
                        stockSeguridad = stock.stockSeguridad,
                        puntoPedido = stock.puntoPedido,
                        cgi = Math.Round(articulo.cgi,4),
                        stockMax = articulo.stockMax
                    });
                }
                return listaArtD;
            }

            public async Task<ArticuloInvDto?> GetArticuloYDatos(long idArticulo)
            {
                var articulo = await _articuloRepository.GetArticuloById(idArticulo);
                if (articulo == null) return null;

                var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(idArticulo);
                if (stock == null || stock.fechaStockFin != null) return null;

                var proveedoresArt = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(idArticulo);
                var proveedorPred = proveedoresArt.FirstOrDefault(pred => pred.predeterminado)?.proveedor?.nombreProveedor ?? "";

                var artDto = new ArticuloInvDto
                {
                    idArticulo = articulo.idArticulo,
                    nombreArticulo = articulo.nombreArticulo,
                    descripcion = articulo.descripcion,
                    modeloInv = articulo.modeloInv.ToString(),
                    demandaEst = articulo.demandaEst,
                    tiempoRevisionDias = articulo.tiempoRevisionDias,
                    costoAlmacen = articulo.costoAlmacen,
                    unidadTemp = articulo.unidadTemp.ToString(),
                    proveedor = proveedorPred,
                    stockActual = stock.stockActual,
                    stockSeguridad = stock.stockSeguridad,
                    puntoPedido = stock.puntoPedido,
                    cgi = Math.Round(articulo.cgi, 4),
                    stockMax = articulo.stockMax
                };
                return artDto;
            }
        #endregion

        #region Calculo LoteFijo_Q
        public async Task CalculoLoteFijoQ(Articulo articulo, StockArticulos stock)
        {
            var proveedoresArticulo = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
            if (!proveedoresArticulo.Any()) return;

            var proveedorArt = proveedoresArticulo.FirstOrDefault(p => p.predeterminado && p.fechaFinProveedorArticulo == null);
            if (proveedorArt == null) return;

            var unidad = articulo.unidadTemp;

            // parametros
            double dEst = articulo.demandaEst; 
            double tiempoEntrega = ModInventarioUtils.ConvertirDesdeDias(proveedorArt.tiempoEntregaDias, unidad);
            double tiempoRevision = ModInventarioUtils.ConvertirDesdeDias(articulo.tiempoRevisionDias, unidad);
            double costoPedido = proveedorArt.costoPedido;
            double costoAlmacen = ModInventarioUtils.ConvertirDesdeAnual(articulo.costoAlmacen, unidad);
            double valSigma = ModInventarioUtils.ConvertirDesdeAnual(articulo.desviacionEstandarDemanda, unidad);
            double Z = ModInventarioUtils.ObtenerZ(articulo.nivelServicio);

            double qOpt = Math.Sqrt((2 * dEst * costoPedido) / costoAlmacen);
            long qOptEnt = (long)Math.Ceiling(qOpt);

            double stockSeg = Z * valSigma * Math.Sqrt(tiempoEntrega);
            long stockSegEnt = (long)Math.Ceiling(stockSeg);

            double ptoPedido = stockSeg + dEst * tiempoEntrega;
            long ptoPedidoEnt = (long)Math.Ceiling(ptoPedido);

            // calcula cgi
            double cgi = CalcularCGI(dEst, proveedorArt.precioUnitario, qOptEnt, costoPedido, costoAlmacen,articulo.unidadTemp);

            articulo.qOptimo = qOptEnt;
            articulo.cgi = cgi;
            stock.stockSeguridad = stockSegEnt;
            stock.puntoPedido = ptoPedidoEnt;
            await _stockArticuloRepository.UpdateAsync(stock);
            await _articuloRepository.UpdateAsync(articulo);
        }
        #endregion

        #region Calculo PeriodoFijo_P
        public async Task CalculoPeriodoFijoP(Articulo articulo, StockArticulos stock)
        {
            var proveedoresArticulo = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
            if (!proveedoresArticulo.Any()) return;

            // proveedor predeterminado 
            var proveedorArt = proveedoresArticulo.FirstOrDefault(p => p.predeterminado && p.fechaFinProveedorArticulo == null);
            if (proveedorArt == null) return;

            var unidad = articulo.unidadTemp;

            // calcula cantidad a pedir
            long cantidadAPedir = await _proveedorArtService.CalcCantidadAPedirP(articulo, proveedorArt);
            if (cantidadAPedir == 0) return;

            // parametros
            double demanda = articulo.demandaEst; 
            double costoUnidad = proveedorArt.precioUnitario;
            double costoPedido = proveedorArt.costoPedido;
            double costoAlmacen = ModInventarioUtils.ConvertirDesdeAnual(articulo.costoAlmacen, unidad); 

            // calcula cgi
            double cgi = CalcularCGI(demanda, costoUnidad, cantidadAPedir, costoPedido, costoAlmacen, unidad);
            articulo.cgi = cgi;
            await _articuloRepository.UpdateAsync(articulo);
        }

        public async Task ControlStockPeriodico(CancellationToken cancellationToken)
        {
            var articulos = await _articuloRepository.GetAllArticulos();

            foreach (var articulo in articulos)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (articulo.modeloInv != ModeloInv.PeriodoFijo_P) continue;

                var stockArticulo = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                if (stockArticulo == null || stockArticulo.fechaStockFin != null) {continue; }

                var proveedoresArticulo = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(articulo.idArticulo);
                if (!proveedoresArticulo.Any()) continue;

                //  selecciona proveedor predeterminado
                var proveedorArt = proveedoresArticulo.FirstOrDefault(pPred => pPred.predeterminado);
                if (proveedorArt == null) continue;
                long idProv = proveedorArt.proveedor!.idProveedor;

                // control por fecha de revision
                if (articulo.fechaRevisionP.HasValue)
                {
                    TimeSpan tiempo = TimeSpan.FromMinutes(articulo.tiempoRevisionDias);
                    DateTime proximaRevision = articulo.fechaRevisionP.Value.Add(tiempo);
                    if (DateTime.Now < proximaRevision) continue;
                }
                var articuloDto = new ArticuloDto {
                    idArticulo = articulo.idArticulo
                    };
                await _ordenCompraService.GenerarOrdenCompra(new List<ArticuloDto> {articuloDto}, idProv);

                // actualizar fecha de revisión
                articulo.fechaRevisionP = DateTime.Now;
                await _articuloRepository.UpdateAsync(articulo);
            }
        }
        #endregion

        #region Calculo CostoGlobalInv
        private double CalcularCGI(double demanda, double costoUnidad, double cantidadPedido, double costoPedido, double costoAlmacen, UnidadTemp? unidad)
        {
            if (cantidadPedido <= 0) return 0;

            // conversion a unidad temporal anual
            double demandaAnual = unidad switch
            {
                UnidadTemp.Semanal => demanda * 52,
                UnidadTemp.Mensual => demanda * 12,
                UnidadTemp.Anual => demanda,
                _ => demanda
            };

            double costoAlmacenAnual = unidad switch
            {
                UnidadTemp.Semanal => costoAlmacen * 52,
                UnidadTemp.Mensual => costoAlmacen * 12,
                UnidadTemp.Anual => costoAlmacen,
                _ => costoAlmacen
            };

            double cgi = (demandaAnual * costoUnidad) +
                        ((demandaAnual / cantidadPedido) * costoPedido) +
                        ((cantidadPedido / 2.0) * costoAlmacenAnual);

            return Math.Round(cgi, 4);
        }
        #endregion

        #region Proveedor Predeterminado
        public async Task<string> EstablecerProveedorPredeterminadoAsync(long idArticulo, long idProveedor)
            {
                // obtener proveedores del articulo
                var proveedoresArticulo = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(idArticulo);
                if (proveedoresArticulo == null || !proveedoresArticulo.Any()) return $"el articulo con Id {idArticulo} no tiene proveedores asignados ";

                var proveedorActual = proveedoresArticulo.FirstOrDefault(pAct =>
                    pAct.proveedor != null &&
                    pAct.proveedor.idProveedor == idProveedor &&
                    pAct.fechaFinProveedorArticulo == null);

                if (proveedorActual == null) return $"el proveedor con Id {idProveedor} no esta asociado actualmente al articulo con Id {idArticulo} ";

                // cargar estados del proveedor
                var estados = await _estProveedorRepository.GetHistorialByProveedorId(idProveedor);
                var estadoActual = estados.FirstOrDefault(estP => estP.fechaFEstadoProveedor == null);

                if (estadoActual == null || estadoActual.proveedorEstado == null || estadoActual.proveedorEstado.idEstadoProveedor != 1)
                {
                 return "el proveedor no se encuentra en estado 'Activo', no puede ser asignado como predeterminado ";
                }

                // salir si el proveedor ya es predeterminado
                if (proveedorActual.predeterminado) return "este proveedor ya está definido como predeterminado para el articulo ";

                // buscar el proveedor predeterminado actual
                var provPredActual = proveedoresArticulo.FirstOrDefault(pPred => pPred.predeterminado);
                if (provPredActual != null)
                {
                    provPredActual.predeterminado = false;
                    await _proveedorArticuloRepository.UpdateAsync(provPredActual);
                }
                // establecer el nuevo proveedor predeterminado
                proveedorActual.predeterminado = true;
                await _proveedorArticuloRepository.UpdateAsync(proveedorActual);
                return $"el proveedor con ID {idProveedor} fue establecido como predeterminado para el articulo con ID {idArticulo}";
            }
        #endregion

        #region Lista productos a reponer

        public async Task<List<ArticuloStockReposicionDto>> ListarArticulosAReponer()
        {
            var articulos = await _articuloRepository.GetAllArticulos();
            var listaArticulosReposicion = new List<ArticuloStockReposicionDto>();

            foreach (var articulo in articulos)
            {
                if (articulo.modeloInv != ModeloInv.LoteFijo_Q) continue;

                var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                if (stock == null || stock.fechaStockFin != null) {continue; }

                // verifica si el stock actual está por debajo del punto de pedido
                if (stock.stockActual > stock.puntoPedido) continue;

                // verificar si no existe una orden vigente
                var estados = new[] { "Pendiente", "Enviada" };
                bool ordenVigente = await _ordenCompraRepository.GetOrdenActual(articulo.idArticulo, estados);

                if (!ordenVigente)
                {
                    listaArticulosReposicion.Add(new ArticuloStockReposicionDto
                    {
                        IdArticulo = articulo.idArticulo,
                        NombreArticulo = articulo.nombreArticulo,
                        StockActual = stock.stockActual,
                        PuntoPedido = stock.puntoPedido
                    });
                }
            }
            return listaArticulosReposicion;
        }
        #endregion

        #region Lista articulos Faltantes
        public async Task<List<ArticuloStockReposicionDto>> ListarArticulosFaltantes()
        {
            var articulos = await _articuloRepository.GetAllArticulos();
            var listaArticulosFaltantes = new List<ArticuloStockReposicionDto>();

            foreach (var articulo in articulos)
            {
                var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                if (stock == null || stock.fechaStockFin != null) {continue; }

                // verifica si el stock actual esta por debajo del stock de seguridad
                if (stock.stockActual < stock.stockSeguridad)
                {
                    listaArticulosFaltantes.Add(new ArticuloStockReposicionDto
                    {
                        IdArticulo = articulo.idArticulo,
                        NombreArticulo = articulo.nombreArticulo ?? "",
                        StockActual = stock.stockActual,
                        StockSeguridad = stock.stockSeguridad
                    });
                }
            }
            return listaArticulosFaltantes;
        }
        #endregion

        #region Lista proveedores por articulo
        public async Task<List<ProveedoresPorArticuloDto>> ListarProveedoresPorArticulo(long idArticulo)
        {
            var proveedoresArticulo = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(idArticulo);
            //if (proveedoresArticulo == null || !proveedoresArticulo.Any()) throw new Exception($"el articulo con Id {idArticulo} no tiene proveedores asignados ");
            var listaProveedoresDto = new List<ProveedoresPorArticuloDto>();

            foreach (var proveedorArt in proveedoresArticulo)
            {
                if (proveedorArt.fechaFinProveedorArticulo != null) continue; 
                var proveedor = proveedorArt.proveedor;
                if (proveedor == null) continue;

                listaProveedoresDto.Add(new ProveedoresPorArticuloDto
                {
                    idProveedor = proveedor.idProveedor,
                    nombreProveedor = proveedor.nombreProveedor ?? "",
                    emailProveedor = proveedor.mail ?? "",
                    telProveedor = proveedor.telefono ?? "",
                    direccionProveedor = proveedor.direccion ?? "",
                    precioUnitario = proveedorArt.precioUnitario,
                    costoPedido = proveedorArt.costoPedido,
                    tiempoEntregaDias = proveedorArt.tiempoEntregaDias,
                    predeterminado = proveedorArt.predeterminado
                });
            }
            return listaProveedoresDto;
        }
        #endregion

        #region cantidad y subtotal para ordenCompra
            public async Task<ResultadoCantidadDto> CalcularCantidadYSubtotal(long idArticulo)
            {
                var articulo = await _articuloRepository.GetByIdAsync(idArticulo);
                if (articulo == null) throw new Exception($"articulo con Id {idArticulo} no encontrado ");

                var proveedorPredeterminado = (await _proveedorArticuloRepository
                .GetAllArticuloProveedorByIdAsync(idArticulo))
                .FirstOrDefault(p => p.predeterminado);
                if (proveedorPredeterminado == null) throw new Exception($"el articulo '{articulo.nombreArticulo}' no tiene proveedor predeterminado ");

                double precioUnitario = proveedorPredeterminado.precioUnitario;
                long cantidad = 0;
                string? aviso = null;

                if (articulo.modeloInv == ModeloInv.LoteFijo_Q)
                {
                    cantidad = articulo.qOptimo;
                }
                else if (articulo.modeloInv == ModeloInv.PeriodoFijo_P)
                {
                    var cantidadCalculada = await _proveedorArtService.CalcCantidadAPedirP(articulo, proveedorPredeterminado);
                    if (cantidadCalculada == 0)
                    {
                        aviso = $"el calculo automatico de cantidad a pedir para el articulo '{articulo.nombreArticulo}' (Id {articulo.idArticulo}) resulta en valores fuera de rango, debe ingresar la cantidad manualmente.";
                        cantidad = 1;
                    } else {cantidad = cantidadCalculada;}
                } else { throw new Exception($"modelo de inventario no reconocido "); }

                double subtotal = Math.Round(cantidad * precioUnitario, 2);
                return new ResultadoCantidadDto
                {
                    cantidad = cantidad,
                    subtotal = subtotal,
                    aviso = aviso
                };
            }
            #endregion

            #region ajuste de inventario
            public async Task AjusteInventarioAsync(ArticuloInvDto dto)
            {
                var articulo = await _articuloRepository.GetArticuloById(dto.idArticulo);
                if (articulo == null){throw new Exception($"articulo con Id {dto.idArticulo} no encontrado ");}
                if (dto.stockActual > dto.stockMax) {throw new Exception("el stock actual no puede superar el stock max. definido ");}
                articulo.stockMax = dto.stockMax;
                await _articuloRepository.UpdateAsync(articulo);

                var stock = await _stockArticuloRepository.getstockActualbyIdArticulo(dto.idArticulo);
                if (stock == null)
                {throw new Exception($"stock no encontrado para el articulo con Id {dto.idArticulo}.");}
                stock.stockActual = dto.stockActual;
                await _stockArticuloRepository.UpdateAsync(stock);
            }
            #endregion
    }
}
#endregion