
using Proyect_InvOperativa.Models.Enums;

namespace Proyect_InvOperativa.Models
{
    public class Articulo
    {
        public virtual long idArticulo { get; set; }
        public virtual string? nombreArticulo { get; set; }
        public virtual string descripcion { get; set; } = "";
        public virtual long demandaDiaria { get; set; }
        public virtual double costoAlmacen { get; set; }
        public virtual long tiempoRevision { get; set; }
        public virtual long qOptimo { get; set; }
        public virtual DateTime? fechaRevisionP { get; set; }
        public virtual double cgi { get; set; }
        public virtual long stockMax { get; set; }
        public virtual ModeloInv? modeloInv { get; set; }
        public virtual CategoriaArt? categoriaArt { get; set; }
        public virtual MaestroArticulo? masterArticulo { get; set; }
    }
}
