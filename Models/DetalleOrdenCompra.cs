namespace Proyect_InvOperativa.Models
{
    public class DetalleOrdenCompra
    {
        public virtual long nDetalleOrdenCompra { get; set; }
        public virtual long cantidadArticulos { get; set; }
        public virtual double precioSubTotal { get; set; }
        public virtual OrdenCompra? ordenCompra { get; set; }

        public virtual Articulo? articulo { get; set; }



    }
}
