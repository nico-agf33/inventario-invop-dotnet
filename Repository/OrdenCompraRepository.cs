using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class OrdenCompraRepository : BaseRepository<OrdenCompra>
    {
        public OrdenCompraRepository(ISessionFactory sessionFactory) : base(sessionFactory)
        {

        }

            public async Task<IEnumerable<OrdenCompra>> GetOrdenesVigentesArt(long idArticulo, string[] estadosOrden)
            {
                using var session = _sessionFactory.OpenSession();

                return await session.Query<DetalleOrdenCompra>()
                    .Where(detC => detC.articulo.idArticulo == idArticulo &&
                      detC.ordenCompra != null &&
                      detC.ordenCompra.ordenEstado != null &&
                      estadosOrden.Contains(detC.ordenCompra.ordenEstado.nombreEstadoOrden))
                    .Select(detC => detC.ordenCompra!)
                    .Distinct()
                    .ToListAsync();
            }

            public async Task<OrdenCompra?> GetOrdenCompraConEstado(long nOrdenCompra)
            {       
                using var session = _sessionFactory.OpenSession();
                return await session.Query<OrdenCompra>()
                .Fetch(x => x.ordenEstado)
                .FirstOrDefaultAsync(x => x.nOrdenCompra == nOrdenCompra);
            }   

        public async Task<OrdenCompraEstado?> GetEstadoOrdenCompra(string nombreEstado)
            {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<OrdenCompraEstado>()
                .FirstOrDefaultAsync(eComp => eComp.nombreEstadoOrden == nombreEstado && eComp.fechaFinEstadoDisponible == null);
            }

            public async Task<bool> GetOrdenActual(long idArticulo, string[] estadosOrden)
            {
                using var session = _sessionFactory.OpenSession();

                return await session.Query<DetalleOrdenCompra>()
                    .Where(detC => detC.articulo.idArticulo == idArticulo &&
                      detC.ordenCompra != null &&
                      detC.ordenCompra.ordenEstado != null &&
                      estadosOrden.Contains(detC.ordenCompra.ordenEstado.nombreEstadoOrden))
        .           AnyAsync();
            }      

            public async Task<List<DetalleOrdenCompra>> GetDetallesByOrdenId(long nOrdenCompra)
            {
                using var session = _sessionFactory.OpenSession();

                return await session.Query<DetalleOrdenCompra>()
                    .Where(dOrden => dOrden.ordenCompra.nOrdenCompra == nOrdenCompra)
                    .Fetch(dOrden => dOrden.articulo) // opcional: incluye info del artículo
                    .ToListAsync();
            }

            public async Task<IEnumerable<OrdenCompra>> GetAllByProveedorIdAsync(long idProveedor)
            {
                using var session = _sessionFactory.OpenSession();
                return await session.Query<OrdenCompra>()
                .Where(ordCompP => ordCompP.proveedor!.idProveedor == idProveedor)
                .Fetch(ordCompP => ordCompP.ordenEstado)
                .ToListAsync();
            }

            public async Task<List<OrdenCompra>> GetOrdenesPorProveedor(long idProveedor)
            {
                using var session = _sessionFactory.OpenSession();
                return await session.Query<OrdenCompra>()
                .Where(ordC => ordC.proveedor != null && ordC.proveedor.idProveedor == idProveedor)
                .Fetch(ordC => ordC.ordenEstado)
                .ToListAsync();
            }

            public async Task<List<OrdenCompra>> GetOrdenesConEstadoYProveedor()
            {
                using var session = _sessionFactory.OpenSession();
                return await session.Query<OrdenCompra>()
                .Fetch(ordC => ordC.proveedor)
                .Fetch(ordC => ordC.ordenEstado)
                .ToListAsync();
            }
    
    }
}
