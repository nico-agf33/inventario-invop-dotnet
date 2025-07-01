using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class StockArticuloRepository : BaseRepository<StockArticulos>
    {
        //private readonly ISessionFactory _sessionFactory;
        public StockArticuloRepository(ISessionFactory sessionFactory) : base(sessionFactory)
        {
            //_sessionFactory = sessionFactory;
        }
        public async Task<StockArticulos?> getstockActualbyIdArticulo(long idArticulo)
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<StockArticulos>()
                .FirstOrDefaultAsync(sArt => sArt.articulo!.idArticulo == idArticulo && sArt.fechaStockFin == null);
        }

        
    }
}

