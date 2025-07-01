using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class ArticuloRepository : BaseRepository<Articulo>
    {
        public ArticuloRepository(ISessionFactory sessionFactory): base(sessionFactory) {}

    public async Task<List<Articulo>> GetAllArticulos()
    {
    using (var session = _sessionFactory.OpenSession())
        {
        var articulos = await session.Query<Articulo>()
            .Fetch(art => art.masterArticulo)
            .ToListAsync();

        return articulos;
        }
    }

    public async Task<Articulo?> GetArticuloById(long id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
        var articulo = await session.Query<Articulo>()
            .Where(art => art.idArticulo == id)
            .Fetch(art => art.masterArticulo)
            .SingleOrDefaultAsync();

        return articulo;
        }
    }
    }

}

