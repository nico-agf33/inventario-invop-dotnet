using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class OrdenCompraMapping: ClassMap<OrdenCompra>
    {
        public OrdenCompraMapping()
        {
            Table("OrdenCompra");
            Id(x => x.nOrdenCompra).GeneratedBy.Identity();
            Map(x => x.totalPagar);
            Map(x => x.fechaOrden);
            References(x => x.ordenEstado)
                .Column("EstadoOrden")
                .Cascade.None();

            References(x => x.proveedor)
                .Column("idProveedor");

        }
    }
}
