namespace Proyect_InvOperativa.Models
{
    public class DescuentoArticulo
    {
        public virtual long nDescuento { get; set; }
        public virtual double porcentajeDesc {  get; set; }
        public virtual long cantidadDesc {  get; set; }
        public virtual DateTime fechaIDescuento { get; set; }
        public virtual DateTime? fechaFDescuento { get; set; }
        public virtual Articulo? articulo { get; set; }
    }
}
