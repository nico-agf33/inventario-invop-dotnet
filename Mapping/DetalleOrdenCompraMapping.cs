using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class DetalleOrdenCompraMapping : ClassMap<DetalleOrdenCompra>
    {
        public DetalleOrdenCompraMapping()
        {
            Table("DetalleOrdenCompra");

            Id(x => x.nDetalleOrdenCompra).GeneratedBy.Identity();
            Map(x => x.cantidadArticulos);
            Map(x => x.precioSubTotal);

            References(x => x.ordenCompra).Column("nOrdenCompra");
            References(x => x.articulo).Column("idArticulo");
        }
    }
}