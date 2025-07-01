using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class ProveedoresRepository:BaseRepository<Proveedor>
    {
        public ProveedoresRepository(ISessionFactory sessionFactory) : base(sessionFactory) 
        {
            
        }

        public async Task<List<Proveedor>> GetAllProveedores()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var proveedores = await session.Query<Proveedor>()
                    .Fetch(prov => prov.masterArticulo)
                    .ToListAsync();

                return proveedores;
            }
        }

        public async Task<Proveedor?> GetProveedorById(long id)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var proveedor = await session.Query<Proveedor>()
                    .Where(prov => prov.idProveedor == id)
                    .Fetch(prov => prov.masterArticulo)
                    .SingleOrDefaultAsync();

                return proveedor;
            }
        }
    }

} 

