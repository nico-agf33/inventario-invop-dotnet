using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class EstadoProveedoresRepository : BaseRepository<EstadoProveedores>
    {
        public EstadoProveedoresRepository(ISessionFactory sessionFactory) : base(sessionFactory) { }

        public async Task<List<EstadoProveedores>> GetHistorialByProveedorId(long idProveedor)
        {
            using var session = _sessionFactory.OpenSession();

            return await session.Query<EstadoProveedores>()
                .Where(estP => estP.proveedor != null && estP.proveedor.idProveedor == idProveedor)
                .Fetch(estP => estP.proveedorEstado) 
                .ToListAsync();
        }
    }
}
