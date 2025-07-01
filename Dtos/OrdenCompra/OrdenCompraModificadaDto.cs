using Proyect_InvOperativa.Dtos.Articulo;

namespace Proyect_InvOperativa.Dtos.OrdenCompra
{
    public class OrdenCompraModificadaDto
    {

            public long nOrdenCompra { get; set; }
            public long idProveedor { get; set; }
            public List<ArticuloCantidadOrdenDto> articulos { get; set; } = new();
        
    }
}
