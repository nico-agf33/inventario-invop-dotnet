namespace Proyect_InvOperativa.Models
{
    public class OrdenCompra
    {
        public virtual long nOrdenCompra {  get; set; }
        public virtual DateTime? fechaOrden {  set; get; }
        public virtual double totalPagar { get; set; }
        public virtual Proveedor? proveedor { get; set; }
        public virtual OrdenCompraEstado? ordenEstado { get; set; }
    }
}
