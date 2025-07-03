﻿using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Dtos.Proveedor;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Repository;

namespace Proyect_InvOperativa.Services
{
    public class ProveedorService
    {
        private readonly ProveedoresRepository _proveedoresRepository;
        private readonly ProveedorArticuloRepository _proveedorArticuloRepository;
        private readonly EstadoProveedoresRepository _estProveedorRepository;
        private readonly ArticuloRepository _articuloRepository;
        private readonly StockArticuloRepository _stockArtRepository;
        private readonly MaestroArticulosRepository _maestroArticuloRepository;
        private readonly OrdenCompraRepository _ordenCompraRepository;
        private readonly ProveedorEstadoRepository _proveedorEstadoRepository;

        public ProveedorService(ProveedorEstadoRepository proveedorEstadoRepository,StockArticuloRepository stockArticuloRepository, OrdenCompraRepository ordenCompraRepository, EstadoProveedoresRepository estProveedorRepository, ArticuloRepository articuloRepository, ProveedoresRepository proveedoresRepository, ProveedorArticuloRepository proveedorArticulo, MaestroArticulosRepository maestroArticulosRepository)
        {
            _proveedorEstadoRepository = proveedorEstadoRepository;
            _articuloRepository = articuloRepository;
            _proveedoresRepository = proveedoresRepository;
            _stockArtRepository = stockArticuloRepository;
            _ordenCompraRepository = ordenCompraRepository;
            _estProveedorRepository = estProveedorRepository;
            _proveedorArticuloRepository = proveedorArticulo;
            _maestroArticuloRepository = maestroArticulosRepository;
        }


        #region ABM Proveedores

        #region CREAR PROVEEDOR
        public async Task<Proveedor> CreateProveedor(ProveedorDto ProveedorDto)
        {
            var maestro = await _maestroArticuloRepository.GetByIdAsync(1);
            var EstProv = await _proveedorEstadoRepository.GetByIdAsync(1);//revisar
            var proveedor_n = new Proveedor()
            {
                nombreProveedor = ProveedorDto.nombreProveedor,
                direccion = ProveedorDto.direccion,
                mail = ProveedorDto.mail,
                telefono = ProveedorDto.telefono,
                masterArticulo = maestro
            };
            var estadoProveedor = new EstadoProveedores()
            {
                nEstado = 1,
                fechaIEstadoProveedor = DateTime.UtcNow,
                fechaFEstadoProveedor = null,
                proveedor = proveedor_n,
                proveedorEstado = EstProv
            };

            var newProveedor = await _proveedoresRepository.AddAsync(proveedor_n);
            var newEstProveedor = await _estProveedorRepository.AddAsync(estadoProveedor);

            return newProveedor;
        }
        #endregion

        #region ACTUALIZAR PROVEEDOR 
        public async Task UpdateProveedor(long idProveedor, ProveedorDto updateProveedorDto)
        {
            var proveedorModificado = await _proveedoresRepository.GetProveedorById(idProveedor);
            if (proveedorModificado is null)
            {
                throw new Exception($"Proveedor con id: {idProveedor} no encontrado. ");
            }

            proveedorModificado.nombreProveedor = updateProveedorDto.nombreProveedor;
            proveedorModificado.mail = updateProveedorDto.mail;
            proveedorModificado.direccion = updateProveedorDto.direccion;
            proveedorModificado.telefono = updateProveedorDto.telefono;
            await _proveedoresRepository.UpdateAsync(proveedorModificado);

        }
        #endregion

        #region ELIMINAR PROVEEDOR
        public async Task DeleteProveedor(long idProveedor)
        {
            var proveedor = await _proveedoresRepository.GetProveedorById(idProveedor);
            if (proveedor is null) throw new Exception($"Proveedor con ID {idProveedor} no encontrado.");

            // obtener historico de estados 
            var historialEstados = await _estProveedorRepository.GetHistorialByProveedorId(idProveedor);
            var estadoActual = historialEstados.FirstOrDefault(est => est.fechaFEstadoProveedor == null);

            if (estadoActual == null) throw new Exception("no se encontro estado vigente para este proveedor ");

            // validar que no sea proveedor predeterminado de ningun articulo
            var aProvArt = await _proveedorArticuloRepository.GetAllByProveedorIdAsync(idProveedor);
            if (aProvArt.Any(pPred => pPred.predeterminado)) throw new Exception("no se puede eliminar el proveedor porque es predeterminado de uno o más artículos ");

            // validar que no tenga ordenes de compra pendientes o en proceso
            var ordenesProveedor = await _ordenCompraRepository.GetAllByProveedorIdAsync(idProveedor);
            var estadosInvalidos = new[] { "Pendiente", "En proceso" };
            if (ordenesProveedor.Any(ordComp => ordComp.ordenEstado != null && estadosInvalidos.Contains(ordComp.ordenEstado.nombreEstadoOrden, StringComparer.OrdinalIgnoreCase))) throw new Exception("el proveedor tiene ordenes de compra pendientes o en proceso, no se puede dar de baja ");

            // obtener estado 'eliminado' 
            var estadoEliminado = await _proveedorEstadoRepository.GetByIdAsync(3);
            if (estadoEliminado == null) throw new Exception("no se encontro el estado 'eliminado' ");

           // cerrar estado actual
            estadoActual.fechaFEstadoProveedor = DateTime.UtcNow;
            await _estProveedorRepository.UpdateAsync(estadoActual);

            // registrar nuevo estado
            var nuevoEstado = new EstadoProveedores
            {
                proveedor = proveedor,
                proveedorEstado = estadoEliminado,
                fechaIEstadoProveedor = DateTime.UtcNow,
                fechaFEstadoProveedor = null
            };
            await _estProveedorRepository.AddAsync(nuevoEstado);

            // cerrar todas las relaciones proveedorArtiulo activas
            foreach (var relProvArt in aProvArt)
            {
                relProvArt.fechaFinProveedorArticulo = DateTime.UtcNow;
                await _proveedorArticuloRepository.UpdateAsync(relProvArt);
            }
            await _proveedoresRepository.UpdateAsync(proveedor);
        }
        #endregion

        #region SUSPENDER PROVEEDOR
        public async Task SuspenderProveedor(long idProveedor)
        {
            var proveedor = await _proveedoresRepository.GetProveedorById(idProveedor);
            if (proveedor == null) throw new Exception($"proveedor con id: {idProveedor} no encontrado ");

            // obtener estado actual vigente
            var historial = await _estProveedorRepository.GetHistorialByProveedorId(idProveedor);
            var estadoActual = historial.FirstOrDefault(est => est.fechaFEstadoProveedor == null);
            if (estadoActual == null) throw new Exception("no se encontro un estado actual para el proveedor ");

            // verificar que no este previamente suspendido
            if (estadoActual.proveedorEstado?.idEstadoProveedor == 2) throw new Exception("el proveedor ya se encuentra suspendido ");

            // validar que no sea proveedor predeterminado de ningun articulo
            var aProvArt = await _proveedorArticuloRepository.GetAllByProveedorIdAsync(idProveedor);
            if (aProvArt.Any(pPred => pPred.predeterminado)) throw new Exception("no se puede suspender el proveedor porque es predeterminado de uno o más artículos ");

            // validar que no tenga ordenes de compra pendientes o en proceso
            var ordenesProveedor = await _ordenCompraRepository.GetAllByProveedorIdAsync(idProveedor);
            var estadosInvalidos = new[] { "Pendiente", "En proceso" };
            if (ordenesProveedor.Any(ordComp => ordComp.ordenEstado != null && estadosInvalidos.Contains(ordComp.ordenEstado.nombreEstadoOrden, StringComparer.OrdinalIgnoreCase))) throw new Exception("el proveedor tiene ordenes de compra pendientes o en proceso, no se puede ser suspendido ");

            // obtener estado 'Suspendido'
            var estadoSuspendido = await _proveedorEstadoRepository.GetByIdAsync(2);
            if (estadoSuspendido == null) throw new Exception("estado 'Suspendido'  no encontrado");

            // cerrar estado anterior
            estadoActual.fechaFEstadoProveedor = DateTime.UtcNow;
            await _estProveedorRepository.UpdateAsync(estadoActual);

            // crear nuevo estado
            var nuevoEstado = new EstadoProveedores
            {
                proveedor = proveedor,
                proveedorEstado = estadoSuspendido,
                fechaIEstadoProveedor = DateTime.UtcNow
            };
            await _estProveedorRepository.AddAsync(nuevoEstado);
            await _proveedoresRepository.UpdateAsync(proveedor);
        }
        #endregion

        #region RESTAURAR PROVEEDOR
        public async Task RestaurarProveedor(long idProveedor)
        {
            var proveedor = await _proveedoresRepository.GetProveedorById(idProveedor);
            if (proveedor is null)
            {
                throw new Exception($"proveedor con id: {idProveedor} no encontrado ");
            }

            // obtener historico de estados del proveedor
            var historialEstados = await _estProveedorRepository.GetHistorialByProveedorId(idProveedor);
            var estadoActual = historialEstados.FirstOrDefault(est => est.fechaFEstadoProveedor == null);

            if (estadoActual == null)
            {
                throw new Exception("no se encontro estado vigente para este proveedor ");
            }

            // verificar si el estado actual es 'suspendido'
            if (estadoActual.proveedorEstado == null || estadoActual.proveedorEstado.idEstadoProveedor != 2)
            {
                throw new Exception("el proveedor no se encuentra en estado 'Suspendido' ");
            }

            // obtener estado 'activo' 
            var estadoActivo = await _proveedorEstadoRepository.GetByIdAsync(1);
            if (estadoActivo == null)
            {
                throw new Exception("no se encontro el estado 'Activo' ");
            }

         // cerrar estado actual
            estadoActual.fechaFEstadoProveedor = DateTime.UtcNow;
            await _estProveedorRepository.UpdateAsync(estadoActual);

            // crear nuevo estado de proveedor
            var nuevoEstado = new EstadoProveedores
            {
                proveedor = proveedor,
                proveedorEstado = estadoActivo,
                fechaIEstadoProveedor = DateTime.UtcNow,
                fechaFEstadoProveedor = null
            };

            await _estProveedorRepository.AddAsync(nuevoEstado);
        }
        #endregion

        #region ALLPROVEEDORES
        public async Task<IEnumerable<Proveedor>> GetAllProveedores()
        {
            var proveedores = await _proveedoresRepository.GetAllProveedores();

            return proveedores;
        }
        #endregion

        #region PROVEEDORBYID
        public async Task<Proveedor?> GetProveedorById(long idProveedor)
        {
            var proveedor = await _proveedoresRepository.GetProveedorById(idProveedor);

            return proveedor;
        }
        #endregion

        #region listar proveedores activos
            public async Task<List<ProveedorDto>> GetProveedoresActivos()
            {
                var proveedores = await _proveedoresRepository.GetAllProveedores();
                var proveedoresActivos = new List<ProveedorDto>();

                foreach (var proveedor in proveedores)
                {
                    var historicoEst = await _estProveedorRepository.GetHistorialByProveedorId(proveedor.idProveedor);
                    var estadoActual = historicoEst.FirstOrDefault(est => est.fechaFEstadoProveedor == null);

                    if (estadoActual != null && estadoActual.proveedorEstado?.idEstadoProveedor == 1) 
                    {
                        proveedoresActivos.Add(new ProveedorDto
                        {
                            idProveedor = proveedor.idProveedor,
                            nombreProveedor = proveedor.nombreProveedor ?? "",
                            direccion = proveedor.direccion ?? "",
                            mail = proveedor.mail ?? "",
                            telefono = proveedor.telefono ?? "",
                        });
                    }
                }
                return proveedoresActivos;
            }
        #endregion

                #region listar proveedores suspendidos
            public async Task<List<ProveedorDto>> GetProveedoresSuspendidos()
            {
                var proveedores = await _proveedoresRepository.GetAllProveedores();
                var proveedoresSuspendidos = new List<ProveedorDto>();

                foreach (var proveedor in proveedores)
                {
                    var historicoEst = await _estProveedorRepository.GetHistorialByProveedorId(proveedor.idProveedor);
                    var estadoActual = historicoEst.FirstOrDefault(est => est.fechaFEstadoProveedor == null);

                    if (estadoActual != null && estadoActual.proveedorEstado?.idEstadoProveedor == 2) 
                    {
                        proveedoresSuspendidos.Add(new ProveedorDto
                        {
                            idProveedor = proveedor.idProveedor,
                            nombreProveedor = proveedor.nombreProveedor ?? "",
                            direccion = proveedor.direccion ?? "",
                            mail = proveedor.mail ?? "",
                            telefono = proveedor.telefono ?? "",
                        });
                    }
                }
                return proveedoresSuspendidos;
            }
        #endregion

        #region lista articulos por Proveedor
            public async Task<List<ProveedorArticuloDto>> GetArticulosPorProveedor(long idProveedor)
            {
                var historial = await _estProveedorRepository.GetHistorialByProveedorId(idProveedor);
                var estadoActual = historial.FirstOrDefault(estP => estP.fechaFEstadoProveedor == null);

                if (estadoActual == null || estadoActual.proveedorEstado?.idEstadoProveedor != 1)
                {
                    return new List<ProveedorArticuloDto>(); 
                }

                // relaciones proveedor-articulo
                var provArt_n = await _proveedorArticuloRepository.GetAllByProveedorIdAsync(idProveedor);
                var listaProvArt = new List<ProveedorArticuloDto>();

                foreach (var provArt in provArt_n)
                {
                    var articulo = provArt.articulo;
                    if (articulo == null) continue;
                    if (provArt.fechaFinProveedorArticulo != null) continue;
                    var stock = await _stockArtRepository.getstockActualbyIdArticulo(articulo.idArticulo);
                    if (stock == null || stock.fechaStockFin != null) continue; 

                    listaProvArt.Add(new ProveedorArticuloDto
                    {
                        idProveedor = idProveedor,
                        idArticulo = articulo.idArticulo,
                        nombreArticulo = articulo.nombreArticulo,
                        precioUnitario = provArt.precioUnitario,
                        tiempoEntregaDias = provArt.tiempoEntregaDias,
                        predeterminado = provArt.predeterminado,
                        fechaFinProveedorArticulo = provArt.fechaFinProveedorArticulo,
                        costoPedido = provArt.costoPedido
                    });
                }
                return listaProvArt;
            }
        #endregion

        #region ALTA PROVEEDOR CON ARTICULOS
        public async Task<string> AltaProveedorConArticulos(ProvConArticulosDto ProvArtDto)
        {
            if (ProvArtDto.articulos == null || !ProvArtDto.articulos.Any()) throw new Exception("debe asignar al menos un articulo al proveedor ");

            // valida que no haya articulos duplicados
            var idDuplicado = ProvArtDto.articulos
            .GroupBy(art => art.idArticulo)
            .Where(idDup => idDup.Count()>1)
            .Select(idDup => idDup.Key)
            .ToList();

            if (idDuplicado.Any())
            {
                string idsArtDup = string.Join(",", idDuplicado);
                throw new Exception($"articulos duplicados -> {idsArtDup}");
            }

            var estadoAlta = await _proveedorEstadoRepository.GetByIdAsync(1); 
            var maestro = await _maestroArticuloRepository.GetByIdAsync(1);
            var nuevoProveedor = new Proveedor
            {
                nombreProveedor = ProvArtDto.proveedor.nombreProveedor,
                direccion = ProvArtDto.proveedor.direccion,
                mail = ProvArtDto.proveedor.mail,
                telefono = ProvArtDto.proveedor.telefono,
                masterArticulo = maestro
            };

            var proveedorGuardado = await _proveedoresRepository.AddAsync(nuevoProveedor);

            foreach (var artDto in ProvArtDto.articulos)
            {
                var articulo = await _articuloRepository.GetArticuloById(artDto.idArticulo);
                if (articulo == null)  throw new Exception($"articulo con Id {artDto.idArticulo} no encontrado ");

                var proveedorArticulo = new ProveedorArticulo
                {
                    proveedor = proveedorGuardado,
                    articulo = articulo,
                    precioUnitario = artDto.precioUnitario,
                    tiempoEntregaDias = artDto.tiempoEntregaDias,
                    costoPedido = artDto.costoPedido,
                    fechaFinProveedorArticulo = artDto.fechaFinProveedorArticulo
                };

            await _proveedorArticuloRepository.AddAsync(proveedorArticulo);
            }
                var estadoProv = new EstadoProveedores
            {
                proveedor = proveedorGuardado,
                proveedorEstado = estadoAlta,
                fechaIEstadoProveedor = DateTime.UtcNow
            };
                await _estProveedorRepository.AddAsync(estadoProv);
                return "proveedor guardado correctamente ";
        }
        #endregion

        #region Historial estados proveedor
            public async Task<IEnumerable<ProveedorHistoricoEstadosDto>> GetHistoricoEstadosProveedor(long idProveedor)
            {
                var proveedor = await _proveedoresRepository.GetProveedorById(idProveedor);
                if (proveedor is null) throw new Exception($"proveedor con Id: {idProveedor} no encontrado.");

                var historicoEstados = await _estProveedorRepository.GetHistorialByProveedorId(idProveedor);
                if (!historicoEstados.Any()) throw new Exception($"el proveedor con Id: {idProveedor} no tiene historial de estados ");

                return historicoEstados.Select(estP => new ProveedorHistoricoEstadosDto
                {
                nombreEstado = estP.proveedorEstado.nombreEstadoProveedor,
                fechaIEstadoProveedor = estP.fechaIEstadoProveedor,
                fechaFEstadoProveedor = estP.fechaFEstadoProveedor
                }).ToList();
            }
            #endregion

        #region listar proveedores + dto
            public async Task<List<ProveedorDto>> GetProveedoresConDto()
            {
                var proveedores = await _proveedoresRepository.GetAllProveedores();
                var proveedoresEnSist = new List<ProveedorDto>();

                foreach (var proveedor in proveedores)
                {
                        proveedoresEnSist.Add(new ProveedorDto
                        {
                            idProveedor = proveedor.idProveedor,
                            nombreProveedor = proveedor.nombreProveedor ?? "",
                            direccion = proveedor.direccion ?? "",
                            mail = proveedor.mail ?? "",
                            telefono = proveedor.telefono ?? "",
                        });
                }
                return proveedoresEnSist;
            }
        #endregion

        #region buscar proveedor predeterminado de articulo
            public async Task<long> GetProvPredeterminadoArt(long idArticulo)
            {
                var proveedoresRelacionados = await _proveedorArticuloRepository.GetAllArticuloProveedorByIdAsync(idArticulo);
                var proveedorPredeterminado = proveedoresRelacionados.FirstOrDefault(pred => pred.predeterminado);
    
                if (proveedorPredeterminado == null) throw new Exception("este articulo no posee proveedor predeterminado ");
                return proveedorPredeterminado.proveedor!.idProveedor;
            }
        #endregion
    }
    #endregion
}