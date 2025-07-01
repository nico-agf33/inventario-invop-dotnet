using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class DetalleOrdenCompraRepository : BaseRepository<DetalleOrdenCompra>
    {
        public DetalleOrdenCompraRepository(ISessionFactory sessionFactory) : base(sessionFactory) { }

    public async Task<List<DetalleOrdenCompra>> GetDetallesByOrdenId(long nOrdenCompra)
    {
        using var session = _sessionFactory.OpenSession();

        return await session.Query<DetalleOrdenCompra>()
            .Where(detOC => detOC.ordenCompra.nOrdenCompra == nOrdenCompra)
            .Fetch(detOC => detOC.articulo) 
            .ToListAsync();
    }

            public async Task<List<DetalleOrdenCompra>> GetDetallesByArticuloId(long idArticulo)
            {
                using var session = _sessionFactory.OpenSession();

                return await session.Query<DetalleOrdenCompra>()
                .Where(det => det.articulo.idArticulo == idArticulo)
                .Fetch(det => det.ordenCompra)
                .ThenFetch(oc => oc.proveedor)
                .Fetch(det => det.ordenCompra)
                .ThenFetch(oc => oc.ordenEstado)
                .ToListAsync();
            }

            public async Task<DetalleOrdenCompra?> GetDetalleByOrdenYArticulo(long nOrdenCompra, long idArticulo)
            {
                using var session = _sessionFactory.OpenSession();
                return await session.Query<DetalleOrdenCompra>()
                .Where(d => d.ordenCompra.nOrdenCompra == nOrdenCompra && d.articulo.idArticulo == idArticulo)
                .Fetch(d => d.articulo)
                .SingleOrDefaultAsync();
            }
    }

    
}

 
