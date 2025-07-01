using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class ProveedorEstadoRepository : BaseRepository<ProveedorEstado>
    {
        public ProveedorEstadoRepository(ISessionFactory sessionFactory) : base(sessionFactory) { }

        public async Task<List<ProveedorEstado>> GetAllEstadosProveedor()
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<ProveedorEstado>().ToListAsync();
        }
    }
}

 
