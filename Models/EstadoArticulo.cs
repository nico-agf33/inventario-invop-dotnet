namespace Proyect_InvOperativa.Models
{
    public class EstadoArticulo
    {
        public virtual long idEstado { get; set; }
        public virtual string? nombreEstado { get; set; }
        public virtual DateTime fechaInicioEstado { get; set; }
        public virtual DateTime fechaFinEstado { get; set; }

    }
}
