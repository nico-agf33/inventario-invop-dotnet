using FluentNHibernate.Mapping;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Mapping
{
    public class ProveedorEstadoMapping:ClassMap<ProveedorEstado>
    {
        public ProveedorEstadoMapping()
        {
            Table("ProveedorEstado");
            Id(x => x.idEstadoProveedor).GeneratedBy.Identity();
            Map(x => x.nombreEstadoProveedor);
            Map(x => x.fechaBajaProveedorEstado);
            
        }
    }
}
