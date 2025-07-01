using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class EstadoProveedoresMapping : ClassMap<EstadoProveedores>
    {
        public EstadoProveedoresMapping()
        {
            Table("EstadoProveedores");
            Id(x => x.nEstado);
            Map(x => x.fechaIEstadoProveedor);
            Map(x => x.fechaFEstadoProveedor);
            References(x => x.proveedorEstado)
                .Column("proveedorEstado")
                .Cascade.None();
            References(x => x.proveedor)
                .Column("proveedor")
                .Cascade.None();
        }
    }
}
