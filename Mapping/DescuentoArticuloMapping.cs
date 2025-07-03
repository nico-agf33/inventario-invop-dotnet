using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class DescuentoArticuloMapping:ClassMap<DescuentoArticulo>
    {
        public DescuentoArticuloMapping()
        {
            Table("DescuentoArticulo");
            Id(x => x.nDescuento)
                .Column("nDescuento")
                .GeneratedBy.Identity();
            Map(x => x.cantidadDesc);
            Map(x => x.porcentajeDesc);
            Map(x => x.fechaIDescuento);
            Map(x => x.fechaFDescuento);
            References(x => x.articulo)
          .Column("idArticulo")
          .Not.Nullable(); 



        }
    }
}
