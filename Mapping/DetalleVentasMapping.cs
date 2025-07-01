using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class DetalleVentasMapping:ClassMap<DetalleVentas>
    {
        public DetalleVentasMapping()
        {
            Table("DetalleVentas");
            Id(x => x.nDetalleVenta).GeneratedBy.Identity();
            Map(x => x.subTotalVenta);
            Map(x => x.cantidad);
            References(x => x.venta)
                .Column("Venta") //se asocia la venta general y los detalles
                .Cascade.None();
            References(x => x.articulo)
                .Column("articulo") 
                .Cascade.None();
        }
    }
}
