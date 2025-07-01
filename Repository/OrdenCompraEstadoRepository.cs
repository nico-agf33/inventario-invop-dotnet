using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class OrdenCompraEstadoRepository:BaseRepository<OrdenCompraEstado>
    {
        public OrdenCompraEstadoRepository(ISessionFactory sessionFactory) : base(sessionFactory)
        {

        }

        public async Task<OrdenCompraEstado?> GetEstado(string nombreEstado)
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<OrdenCompraEstado>()
                .FirstOrDefaultAsync(OC_est => OC_est.nombreEstadoOrden == nombreEstado && OC_est.fechaFinEstadoDisponible == null);
        }

        public async Task<List<OrdenCompraEstado>> GetAllEstadosOrden()
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<OrdenCompraEstado>().ToListAsync();
        }
    
    }
}
