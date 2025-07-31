using Proyect_InvOperativa.Models.Enums;

public class DemandaDto
{
    public double demanda { get; set; }
    public double? desviacionEstandarPeriodo { get; set; }
    public List<DemandaTablaDto> valoresTabla { get; set; }
    public List<DemandaPuntoXYDto>? puntosXY { get; set; }
    public double? s2_rr { get; set; }
    public double? s2_rc { get; set; }
    public double? s_rr { get; set; }
    public double? s_rc { get; set; }
    public double? r0 { get; set; }
}