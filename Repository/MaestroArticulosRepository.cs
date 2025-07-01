using NHibernate;
using NHibernate.Linq;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Repository
{
    public class MaestroArticulosRepository : BaseRepository<MaestroArticulo>
    {
        public MaestroArticulosRepository (ISessionFactory sessionFactory) :base(sessionFactory) { 

        }
    


} }
