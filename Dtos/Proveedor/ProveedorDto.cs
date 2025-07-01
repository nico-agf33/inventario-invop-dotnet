namespace Proyect_InvOperativa.Dtos.Proveedor

{
    public class ProveedorDto
    {
        public  long idProveedor {  get; set; } 
        public  string nombreProveedor { get; set; } = "";
        public  string direccion { get; set; } 
        public  string mail { get; set; } 
        public  string telefono { get; set; } 
        public long? masterArticulo { get; set; }
    }
}
