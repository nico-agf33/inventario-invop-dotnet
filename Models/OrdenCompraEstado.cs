namespace Proyect_InvOperativa.Models
{
    public class OrdenCompraEstado
    {
       public virtual string? nombreEstadoOrden {  get; set; }
        public virtual long idOrdenCompraEstado { get; set; }
        public virtual DateTime? fechaFinEstadoDisponible { get; set; }

      
    }
}