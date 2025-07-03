
using Proyect_InvOperativa.Models.Enums;

namespace Proyect_InvOperativa.Models
{
    public class Articulo
    {
        public virtual long idArticulo { get; set; }
        public virtual string? nombreArticulo { get; set; }
        public virtual string descripcion { get; set; } = "";
        public virtual long demandaEst { get; set; }
        public virtual double costoAlmacen { get; set; }
        public virtual long tiempoRevisionDias { get; set; }
        public virtual long qOptimo { get; set; }
        public virtual double precioVenta { get; set; }
        public virtual DateTime? fechaRevisionP { get; set; }
        public virtual double cgi { get; set; }
        public virtual long stockMax { get; set; }
        public virtual ModeloInv? modeloInv { get; set; }
        public virtual double nivelServicio { get; set; }
        public virtual double desviacionEstandarDemanda { get; set; }
        public virtual UnidadTemp? unidadTemp { get; set; }
        public virtual MaestroArticulo? masterArticulo { get; set; }
    }
}
