
public class DemandaPuntoXYDto
{
    public int x { get; set; } // período relativo (1,2,...,n)
    public double? yReal { get; set; } // valor real si está disponible
    public double yEstimado { get; set; } // valor estimado por el modelo
}