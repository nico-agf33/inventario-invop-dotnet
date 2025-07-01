using Proyect_InvOperativa.Dtos.Articulo;

namespace Proyect_InvOperativa.Dtos.Proveedor

{
    public class ProvConArticulosDto
    {
      public ProveedorDto proveedor { get; set; }
      public List<ProveedorArticuloDto> articulos { get; set; }
    }   
}
