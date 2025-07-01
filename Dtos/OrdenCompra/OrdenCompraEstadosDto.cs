namespace Proyect_InvOperativa.Dtos.OrdenCompra
{
    public class OrdenCompraEstadosDto
    {
        public string nombreEstadoOrden { get; set; } = "";
        public long idOrdenCompraEstado { get; set; }
        public DateTime? fechaFinEstadoDisponible { get; set; }
        
        
    }
}
