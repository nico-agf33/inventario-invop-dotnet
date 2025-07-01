using System;
namespace Proyect_InvOperativa.Models
{
    public class ProveedorEstado
    {
        public virtual string nombreEstadoProveedor { get; set; } = "";
        public virtual long idEstadoProveedor { get; set; }
        public virtual DateTime? fechaBajaProveedorEstado { get; set; }
        
      
    }
}