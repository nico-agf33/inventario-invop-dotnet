
namespace Proyect_InvOperativa.Models {
    public class DetalleVentas
    {
        public virtual long cantidad { get; set; }
        public virtual long nDetalleVenta { get; set; }
        public virtual double subTotalVenta { get; set; }
        public virtual Ventas? venta {  get; set; }
        public virtual Articulo? articulo { get; set; }
       
      
    }
}