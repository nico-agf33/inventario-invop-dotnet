using Proyect_InvOperativa.Dtos.Articulo;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Dtos.OrdenCompra
{
    public class OrdenCompraGeneradaDto
    {
       public long idProveedor { get; set; }
       public List<ArticuloDto> articulos { get; set; } = new();
    }
}
