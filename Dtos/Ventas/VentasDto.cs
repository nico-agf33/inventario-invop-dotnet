namespace Proyect_InvOperativa.Dtos.Ventas
{
    public class VentasDto
    {
        public long? nVenta { get; set; }
        public string? descripcionVenta { get; set; }
        public double? totalVenta { get; set; }
        public DetalleVentasDto[] detalles { get; set; } = [];

    }
}
