namespace Proyect_InvOperativa.Models
{
    public class ProveedorArticulo
    {
        public virtual long idProveedorArticulo { get; set; }
        public virtual double precioUnitario { get; set; }
        public virtual double costoPedido { get; set; }
        public virtual long tiempoEntregaDias { get; set; }
        public virtual bool predeterminado { get; set; }
        public virtual DateTime? fechaFinProveedorArticulo { get; set; }
        public virtual Articulo? articulo { get; set; }
        public virtual Proveedor? proveedor { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (ProveedorArticulo)obj;

            // Compara las claves compuestas (articulo y proveedor)
            return Equals(articulo, other.articulo) && Equals(proveedor, other.proveedor);
        }

        public override int GetHashCode()
        {
            unchecked // para permitir overflow
            {
                int hash = 17;
                hash = hash * 23 + (articulo != null ? articulo.GetHashCode() : 0);
                hash = hash * 23 + (proveedor != null ? proveedor.GetHashCode() : 0);
                return hash;
            }
        }

    }
}
