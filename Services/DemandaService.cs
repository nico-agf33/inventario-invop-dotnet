using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Dtos;
using Proyect_InvOperativa.Dtos.OrdenCompra;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Models.Enums;
using Proyect_InvOperativa.Utils;

namespace Proyect_InvOperativa.Services
{
    public class DemandaService
    {
        public readonly DetalleVentasRepository _detVentasRepository;

        public DemandaService(DetalleVentasRepository detVentasRepository)
        {
            _detVentasRepository = detVentasRepository;
        }

        public async Task<(double demanda, double desviacionAnual)> CalcularDemandaYDesviacionAnual(Articulo articulo)
        {
            await ValidarExistenciaVentas(articulo);
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);

            // agrupa ventas por año y mes
            var ventasPorMes = detalles
            .Where(dVent => dVent.venta != null)
            .GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
            .Select(gr => new
            {
                Año = gr.Key.Year,
                Mes = gr.Key.Month,
                Cantidad = gr.Sum(x => x.cantidad)
            })
            .OrderByDescending(gr => new DateTime(gr.Año, gr.Mes, 1))
            .Take(6) 
            .Reverse() 
            .ToList();
            if (ventasPorMes.Count == 0) return (0, 0);

            // pesos ponderados 
            var c_i = new List<double> { 1, 2, 3, 4, 5, 6 }; 
            c_i = c_i.Skip(6 - ventasPorMes.Count).ToList(); 

            double sumaC_i = c_i.Sum();
            var cantidades = ventasPorMes.Select(vMes => (double)vMes.Cantidad).ToList();

            // media ponderada mensual
            double mediaPonderada = cantidades
            .Select((x, i) => x * c_i[i])
            .Sum() / sumaC_i;

            // desviacion estandar ponderada mensual
            double varianzaPonderada = cantidades
            .Select((x, i) => c_i[i] * Math.Pow(x - mediaPonderada, 2))
            .Sum() / sumaC_i;

            double desviacionMensual = Math.Sqrt(varianzaPonderada);

            // convierte desviacion mensual en anual
            double desviacionAnual = desviacionMensual * Math.Sqrt(12);

            // expresa mediaPonderada segun unidadTemp
            double demanda = articulo.unidadTemp switch
            {
                UnidadTemp.Semanal => mediaPonderada / 4.0,
                UnidadTemp.Mensual => mediaPonderada,
                UnidadTemp.Anual => mediaPonderada * 12.0,
                _ => mediaPonderada
            };

            return (Math.Round(demanda, 4), Math.Round(desviacionAnual, 4));
        }
     
        public async Task<(double rmse, double mae, double mape)> CalcularErroresDemanda(Articulo articulo)
        {
            await ValidarExistenciaVentas(articulo);
            var (demandaEstimacion, _) = await CalcularDemandaYDesviacionAnual(articulo);
            double demandaMensual = ModInventarioUtils.ConvertirAMensual(demandaEstimacion, articulo.unidadTemp);
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);

            // agrupa ventas por mes
            var ventas_semestre = DateTime.Now.AddMonths(-6);

            var ventasPorMes = detalles
            .Where(dVent => dVent.venta != null && dVent.venta.fechaVenta >= ventas_semestre)
            .GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
            .OrderByDescending(gr => gr.Key.Year).ThenByDescending(gr => gr.Key.Month)
            .Take(6)
            .Reverse()
            .Select(gr => gr.Sum(x => x.cantidad))
            .ToList();
            if (ventasPorMes.Count == 0) return (0, 0, 0);

            var estimados = Enumerable.Repeat(demandaMensual, ventasPorMes.Count).ToList();

            // calculos de error
            var errAbsolutos = ventasPorMes
            .Select((x, i) => Math.Abs(x - estimados[i]))
            .ToList();

            var errCuadrados = ventasPorMes
            .Select((x, i) => Math.Pow(x - estimados[i], 2))
            .ToList();

            var errPorcentuales = ventasPorMes
            .Select((x, i) => x == 0 ? 0 : Math.Abs((x - estimados[i]) / x) * 100)
            .ToList();

            double mae = errAbsolutos.Average();
            double rmse = Math.Sqrt(errCuadrados.Average());
            double mape = errPorcentuales.Average();
            return (Math.Round(rmse, 4), Math.Round(mae, 4), Math.Round(mape, 4));
        }

        private async Task ValidarExistenciaVentas(Articulo articulo)
        {
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);
            if (detalles == null || !detalles.Any(detV => detV.venta != null && detV.cantidad > 0)) throw new InvalidOperationException($"no existen ventas validas para el articulo '{articulo.nombreArticulo}' (Id {articulo.idArticulo})");
        }
    }
}
