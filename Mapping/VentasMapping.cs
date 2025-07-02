using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class VentasMapping:ClassMap<Ventas>
    {
        public VentasMapping()
        {
            Table("Venta");
            Id(x => x.nVenta).GeneratedBy.Identity();
            Map(x => x.descripcionVenta);
            Map(x => x.totalVenta);
            Map(x => x.fechaVenta);
        }
    }
}
