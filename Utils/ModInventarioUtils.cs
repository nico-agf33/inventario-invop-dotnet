using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using Proyect_InvOperativa.Models.Enums;

namespace Proyect_InvOperativa.Utils
{
    public static class ModInventarioUtils
    {
        public static double ObtenerZ(double nServicio)
        {
            if (nServicio<=0.0 || nServicio>=100.0)
                throw new ArgumentException(
                    "el nivel de servicio se debe especificar en valores entre 0 y 100"
                );

            double p = nServicio/100; 
            double Z = Normal.InvCDF(0,1,p); 
            return Z;
        }

        public static double ConvertirDesdeAnual(double valorAnual, UnidadTemp? unidad)
        {
            return unidad switch
            {
                UnidadTemp.Semanal => valorAnual / 52.0,
                UnidadTemp.Mensual => valorAnual / 12.0,
                UnidadTemp.Anual => valorAnual,
                _ => valorAnual
            };
        }

        public static double ConvertirDesdeDias(double dias, UnidadTemp? unidad)
        {
            return unidad switch
            {
                UnidadTemp.Semanal => dias / 7.0,
                UnidadTemp.Mensual => dias / 30.0,
                UnidadTemp.Anual => dias / 365.0,
                _ => dias
            };
        }

        public static double ConvertirAMensual(double valor, UnidadTemp? unidad)
        {
            return unidad switch
            {
                UnidadTemp.Semanal => valor * 4.33,
                UnidadTemp.Anual => valor / 12.0,
                UnidadTemp.Mensual => valor,
                _ => valor
            };
        }

	public static double ConvertirDesvEstandarDesdeAnual(double sigmaAnual, UnidadTemp? unidad)
	{
	    return unidad switch
	    {
		UnidadTemp.Semanal => sigmaAnual / Math.Sqrt(52.0),
		UnidadTemp.Mensual => sigmaAnual / Math.Sqrt(12.0),
		UnidadTemp.Anual => sigmaAnual,
		_ => sigmaAnual
	    };
	}
    }
    
}