using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class DetalleVentasRepository : BaseRepository<DetalleVentas>
    {
        public DetalleVentasRepository(ISessionFactory sessionFactory) : base(sessionFactory) { }

        public async Task<List<DetalleVentas>> GetAllDetalleVentas()
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<DetalleVentas>().ToListAsync();
        }

        public async Task<List<DetalleVentas>> GetByArticuloIdAsync(long articuloId)
        {
                using var session = _sessionFactory.OpenSession();

                return await session.Query<DetalleVentas>()
                .Where(dVent => dVent.articulo.idArticulo == articuloId)
                .Fetch(dVent => dVent.venta) 
                .ToListAsync();
        }

    }
}

 
