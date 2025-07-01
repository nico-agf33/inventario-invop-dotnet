using Proyect_InvOperativa.Models.Enums;

namespace Proyect_InvOperativa.Dtos.Articulo
{
    public class ArticuloDto
    {
        public long idArticulo { get; set; } 
        public string? nombreArticulo { get; set; }
        public string descripcion { get; set; } = "";
        public long modeloInv { get; set; }
        public long demandaDiaria { get; set; }
        public double costoAlmacen { get; set; }
        public long categoriaArt { get; set; }
        public long tiempoRevision {get; set;}
        public long idMaster { get; set; }
        public long stockMax { get; set; }
    }
}