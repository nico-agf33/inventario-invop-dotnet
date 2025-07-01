using Proyect_InvOperativa.Models.Enums;

public class ArticuloInvDto
{
    public long idArticulo { get; set; }
    public string nombreArticulo { get; set; } = "";
    public string descripcion { get; set; } = "";
    public string modeloInv { get; set; } = "";
    public string categoriaArt { get; set; } = "";
    public string proveedor { get; set; } = "";
    public  long demandaDiaria { get; set; }
    public  double costoAlmacen { get; set; }
    public  long tiempoRevision { get; set; }
    public long stockActual { get; set; }
    public long stockSeguridad { get; set; }
    public long puntoPedido { get; set; }
    public long stockMax { get; set; }
    public double cgi { get; set; }
}