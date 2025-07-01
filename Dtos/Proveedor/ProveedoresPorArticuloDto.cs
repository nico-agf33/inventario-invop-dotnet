namespace Proyect_InvOperativa.Dtos.Proveedor

{
    public class ProveedoresPorArticuloDto
    {
        public string nombreProveedor { get; set; } = "";
        public long idProveedor { get; set; }
        public string? emailProveedor { get; set; }
        public string? telProveedor { get; set; }
        public string? direccionProveedor { get; set; }
        public double precioUnitario { get; set; }
        public double costoPedido { get; set; }
        public long tiempoEntregaDias { get; set; }
        public bool predeterminado { get; set; }
    }
}
