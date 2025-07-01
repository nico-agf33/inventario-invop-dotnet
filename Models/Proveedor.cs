namespace Proyect_InvOperativa.Models
{
    public class Proveedor
    {
        public virtual string nombreProveedor { get; set; } = "";
        public virtual long idProveedor { get; set; }
        public virtual string? direccion {  get; set; }
        public virtual string? mail { get; set; }
        public virtual string? telefono {  get; set; }
        public virtual MaestroArticulo? masterArticulo { get; set; }
        


    }
}
