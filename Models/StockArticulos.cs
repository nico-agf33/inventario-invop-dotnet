namespace Proyect_InvOperativa.Models
{
    public class StockArticulos
    {
        public virtual long nStock { get; set; }
        public virtual long stockSeguridad {  get; set; }
        public virtual long stockActual {  get; set; }
        public virtual long puntoPedido {  get; set; }
        public virtual bool control {  get; set; }
        public virtual DateTime fechaStockInicio { get; set; }
        public virtual DateTime? fechaStockFin { get; set; }
        public virtual Articulo? articulo { get; set; }
    }
}
