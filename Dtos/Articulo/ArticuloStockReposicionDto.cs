namespace Proyect_InvOperativa.Dtos.Articulo;

public class ArticuloStockReposicionDto
{
    public long IdArticulo { get; set; }
    public string NombreArticulo { get; set; } = "";
    public long StockActual { get; set; }
    public long PuntoPedido { get; set; }
    public long StockSeguridad { get; set; }
}