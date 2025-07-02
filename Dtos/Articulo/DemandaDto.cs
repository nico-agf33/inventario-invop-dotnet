using Proyect_InvOperativa.Models.Enums;

public class DemandaDto
{
    public double demanda { get; set; }
    public double? desviacionEstandarPeriodo { get; set; }
    public List<DemandaTablaDto> valoresTabla { get; set; }
    public List<DemandaPuntoXYDto>? puntosXY { get; set; }
    public double? s2rr { get; set; }
    public double? s2rc { get; set; }
    public double? r0 { get; set; }
}