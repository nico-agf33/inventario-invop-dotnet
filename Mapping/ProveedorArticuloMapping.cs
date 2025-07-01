using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class ProveedorArticuloMapping : ClassMap<ProveedorArticulo>
    {
        public ProveedorArticuloMapping()
        {
            Table("ProveedorArticulo");
            CompositeId()
                .KeyReference(x => x.proveedor, "idProveedor")
                .KeyReference(x => x.articulo, "idArticulo");

            Id(x => x.idProveedorArticulo).GeneratedBy.Identity();
            Map(x => x.precioUnitario);
            Map(x => x.costoPedido);
            Map(x => x.tiempoEntregaDias);
            Map(x => x.predeterminado).Not.Nullable().Default("false");
            Map(x => x.fechaFinProveedorArticulo);

            References(x => x.articulo)
               .Column("articuloAsociado")
               .Cascade.None();

            References(x => x.proveedor)
               .Column("proveedorAsociado")
               .Cascade.None();

            //ASI LO TENIA, LO COMENTO POR LAS DUDAS DEJE LA VERSION DE NICO
            //References(x => x.articulo)
            //   .Column("articuloAsociado")
            //   .Cascade.None();

            //References(x => x.proveedor)
            //    .Column("proveedor")
            //    .Cascade.None();
        }
    }
}
