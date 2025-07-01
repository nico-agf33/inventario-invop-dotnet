using System;
namespace Proyect_InvOperativa.Models
{
	public class EstadoProveedores
	{
		public virtual long nEstado { get; set; }
		public virtual DateTime? fechaIEstadoProveedor { get; set; }
		public virtual DateTime? fechaFEstadoProveedor { get; set; }
		public virtual ProveedorEstado? proveedorEstado { get; set; }
        public virtual Proveedor? proveedor { get; set; }
       
	}
}
