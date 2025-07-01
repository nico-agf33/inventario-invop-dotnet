using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class ProveedorMapping:ClassMap<Proveedor>
    {
        public ProveedorMapping()
        {
            Table("Proveedor");
            Id(x => x.idProveedor).GeneratedBy.Identity();
            Map(x => x.nombreProveedor);
            Map(x => x.direccion);
            Map(x => x.telefono);
            Map(x => x.mail);

            References(x => x.masterArticulo)
                .Column("idMaestroArticulo")
                .Cascade.None();
            
        }
    }
}
