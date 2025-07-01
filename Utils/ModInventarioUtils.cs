using Proyect_InvOperativa.Models.Enums;

namespace Proyect_InvOperativa.Utils
{
    public static class ModInventarioUtils
    {
            public static (double Z, double sigma) ObtenerZySigma(CategoriaArt? categoria, double tiempo_p)
            {
                double Z = 1.64485363; // nivel de servicio esperado (z) para 0,95

                if (categoria == null) throw new ArgumentException("Categoria no encontrada");
                double val_Sigma = categoria switch
                {
                    CategoriaArt.Categoria_A => (6.0+2.0)/2.0,
                    CategoriaArt.Categoria_B => (4.0+1.0)/2.0,
                    CategoriaArt.Categoria_C => (0.2+1.0)/2.0,
                    CategoriaArt.Categoria_D => (0.1+1.0)/2.0,
                    _ => throw new ArgumentException("Categoría no válida")
                };

                double sigma = val_Sigma * Math.Sqrt(tiempo_p);
                return (Z, sigma);
            }
    }
}