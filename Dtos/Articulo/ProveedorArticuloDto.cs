namespace Proyect_InvOperativa.Dtos.Articulo
{
    public class ProveedorArticuloDto
    {
        public long idProveedor { get; set; }
        public string? nombreArticulo { get; set; }
        public double precioUnitario { get; set; }
        public long tiempoEntregaDias { get; set; }
        public long idArticulo { get; set; }
        public Boolean predeterminado { get; set; } = false;
        public DateTime? fechaFinProveedorArticulo { get; set; }
        public double costoPedido { get; set; }
    }
}
