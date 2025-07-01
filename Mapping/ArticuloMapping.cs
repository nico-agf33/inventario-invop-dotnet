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
            Map(x => x.demandaDiaria);
            Map(x => x.costoAlmacen);
            Map(x => x.tiempoRevision);
            Map(x => x.qOptimo);
            Map(x => x.stockMax);
            Map(x => x.fechaRevisionP);
            Map(x => x.cgi);
            Map(x => x.modeloInv).CustomType<ModeloInv>();
            Map(x => x.categoriaArt).CustomType<CategoriaArt>();

            References(x => x.masterArticulo)
                .Column("idMaestroArticulo")
                .Cascade.None();



        }
    }
}
