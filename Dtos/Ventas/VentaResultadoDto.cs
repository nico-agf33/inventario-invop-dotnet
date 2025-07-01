using Venta = Proyect_InvOperativa.Models.Ventas;
namespace Proyect_InvOperativa.Dtos.Ventas
{
    public class VentaResultadoDto
    {
        public Venta venta { get; set; }
        public List<string> advertencias { get; set; } = new();

    }
}
