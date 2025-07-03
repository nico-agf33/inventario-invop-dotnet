using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Models.Enums;
namespace Proyect_InvOperativa.Mapping
{
    public class ArticuloMapping : ClassMap<Articulo>
    {
        public ArticuloMapping()
        {
            Table("Articulos");
            Id(x => x.idArticulo).GeneratedBy.Identity();
            Map(x => x.nombreArticulo);
            Map(x => x.descripcion);
            Map(x => x.demandaEst);
            Map(x => x.costoAlmacen);
            Map(x => x.tiempoRevisionDias);
            Map(x => x.qOptimo);
            Map(x => x.precioVenta);
            Map(x => x.stockMax);
            Map(x => x.fechaRevisionP);
            Map(x => x.cgi);
            Map(x => x.nivelServicio);
            Map(x => x.desviacionEstandarDemanda);            
            Map(x => x.modeloInv).CustomType<ModeloInv>();
            Map(x => x.unidadTemp).CustomType<UnidadTemp>();

            References(x => x.masterArticulo)
                .Column("idMaestroArticulo")
                .Cascade.None();



        }
    }
}
