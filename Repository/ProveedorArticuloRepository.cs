using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class ProveedorArticuloRepository : BaseRepository<ProveedorArticulo>
    {
        public ProveedorArticuloRepository(ISessionFactory sessionFactory) : base(sessionFactory) { }

        public async Task<IEnumerable<ProveedorArticulo>> GetAllArticuloProveedorByIdAsync(long idArticulo)
        {
            using var session = _sessionFactory.OpenSession();

            return await session.Query<ProveedorArticulo>()
            .Where(pArt => pArt.articulo!.idArticulo == idArticulo)
            .Fetch(pArt => pArt.proveedor)
            .ToListAsync();
        }

        public async Task<ProveedorArticulo?> GetProvArtByIdsAsync(long idArticulo, long idProveedor)
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<ProveedorArticulo>()
            .FirstOrDefaultAsync(prArt =>
            prArt.articulo!.idArticulo == idArticulo &&
            prArt.proveedor!.idProveedor == idProveedor);
        }

        public async Task<IEnumerable<ProveedorArticulo>> GetAllByProveedorIdAsync(long idProveedor)
        {
            using var session = _sessionFactory.OpenSession();
            return await session.Query<ProveedorArticulo>()
            .Where(prArt => prArt.proveedor!.idProveedor == idProveedor && prArt.fechaFinProveedorArticulo == null)
            .Fetch(prArt=>prArt.articulo)
            .ToListAsync();
        }
    }
}
