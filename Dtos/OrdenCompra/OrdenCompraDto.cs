using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Dtos.OrdenCompra
{
    public class OrdenCompraDto
    {
        public  long nOrdenCompra {  get; set; }
        public  DateTime? fechaOrden {  set; get; }
        public  double totalPagar { get; set; }
        public  string proveedor { get; set; }
        public  string ordenEstado { get; set; }
    }
}
