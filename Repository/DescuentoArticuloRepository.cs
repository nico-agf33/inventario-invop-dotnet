using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class DescuentoArticuloRepository : BaseRepository<DescuentoArticulo>
    {

        public DescuentoArticuloRepository(ISessionFactory sessionFactory) : base(sessionFactory)
        {

        }

        public async Task<DescuentoArticulo?> getdescActualbyIdArticulo(long idArticulo)
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<DescuentoArticulo>()
                .FirstOrDefaultAsync(dArt => dArt.articulo!.idArticulo == idArticulo && dArt.fechaFDescuento == null);
        }

        
    }
}

