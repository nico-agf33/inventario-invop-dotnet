namespace Proyect_InvOperativa.Dtos.Ventas
{
    public class ArtVentasDto
    {
        public long nVenta { get; set; }
        public long cantidadVendida { get; set; }
        public double subtotal { get; set; }
        public DateTime? fechaVenta { get; set; }
    }
}
