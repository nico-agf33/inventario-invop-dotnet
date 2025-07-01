using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class MaestroArticuloMapping:ClassMap<MaestroArticulo>
    {
        public MaestroArticuloMapping()
        {
            Table("MaestroArticulo");
            Id(x => x.idMaestroArticulo).GeneratedBy.Identity();
            Map(x => x.nombreMaestro);
        }
    }
}
