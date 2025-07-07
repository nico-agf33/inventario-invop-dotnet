using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Dtos;
using Proyect_InvOperativa.Dtos.OrdenCompra;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Models.Enums;
using Proyect_InvOperativa.Utils;
using System.Globalization;

namespace Proyect_InvOperativa.Services
{
    public class DemandaService
    {
        public readonly DetalleVentasRepository _detVentasRepository;
        public readonly MaestroArticulosRepository _maestroArtRepository;
        public readonly ArticuloRepository _artRepository;

        public DemandaService(DetalleVentasRepository detVentasRepository,ArticuloRepository artRepository,MaestroArticulosRepository maestroArtRepository)
        {
            _detVentasRepository = detVentasRepository;
            _maestroArtRepository = maestroArtRepository;
            _artRepository = artRepository;
        }

        #region metodo receptor
        public async Task<DemandaDto> CalcDemandaYDesviacion(long idArticulo, long tipoPrediccion, long periodo, double alfa)
        {
            var articulo = await _artRepository.GetArticuloById(idArticulo)  ?? throw new Exception($"articulo con Id {idArticulo} no encontrado");

            await ValidarExistenciaVentas(articulo);

            double demanda;
            double desviacionEstandarPeriodo = 0;
            double? s2rr = null;
            double? s2rc = null;
            double? r0 = null;
            List<DemandaTablaDto> entradas = new List<DemandaTablaDto>();
            List<DemandaPuntoXYDto>? puntos = null;

            switch (tipoPrediccion)
            {
                case 1:
                    (demanda, desviacionEstandarPeriodo,entradas) = await CalcDemandaPromedioMovil(articulo, periodo);
                    break;

                case 2:
                    (demanda, desviacionEstandarPeriodo,entradas) = await CalcDemandaPromedioMovilPonderado(articulo, periodo);
                    break;

                case 3:
                    (demanda, desviacionEstandarPeriodo,entradas) = await CalcDemandaSuavizacionExp(articulo, periodo, alfa);
                    break;

                case 4:
                    (demanda, s2rr, s2rc, r0,puntos) = await CalcDemandaRegLineal(articulo, periodo);
                    break;

                default:
                    throw new ArgumentException($"tipoPrediccion = {tipoPrediccion} no es un valor correcto");
            }

            return new DemandaDto
            {
                demanda = Math.Round(demanda, 4),
                desviacionEstandarPeriodo = Math.Round(desviacionEstandarPeriodo, 4),
                s2rr = s2rr.HasValue ? Math.Round(s2rr.Value, 4) : null,
                s2rc = s2rc.HasValue ? Math.Round(s2rc.Value, 4) : null,
                r0 = r0.HasValue ? Math.Round(r0.Value, 4) : null,
                valoresTabla = entradas,
                puntosXY = puntos
            };
        }
        #endregion

        #region promedio movil
        public async Task<(double demanda, double desviacionEstandarPeriodo, List<DemandaTablaDto> entradas)> CalcDemandaPromedioMovil(Articulo articulo, long periodo)
        {
            int n = periodo switch
            {
                1 => 3,
                2 => 6,
                3 => 12,
                _ => throw new ArgumentException("el periodo ingresado no es válido")
            };
            await ValidarExistenciaVentas(articulo);
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);

            var ventasAgrupadas = detalles
            .Where(dVent => dVent.venta != null && dVent.venta.fechaVenta < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            .GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
            .OrderByDescending(gr => new DateTime(gr.Key.Year, gr.Key.Month, 1))
            .Take(n)
            .Reverse()
            .Select(gr => new
            {
                Fecha = new DateTime(gr.Key.Year, gr.Key.Month, 1),
                Cantidad = gr.Sum(x => x.cantidad)
            })
            .ToList();
            if (ventasAgrupadas.Count < n)  throw new Exception($"no hay suficientes datos de ventas para calcular promedio movil de {n} meses para el articulo '{articulo.nombreArticulo}'");

            var entradas = ventasAgrupadas
            .Select(v => new DemandaTablaDto
            {
                mes = v.Fecha.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                cantidad = v.Cantidad
            })
            .ToList();
            var ventasPorMes = ventasAgrupadas.Select(v => v.Cantidad).ToList();

            // promedio movil
            double demanda = ventasPorMes.Average();

            // desviacion estandar
            double varianza = ventasPorMes
            .Select(vent => Math.Pow(vent - demanda, 2))
            .Average();
            double desviacionEstandar = Math.Sqrt(varianza);

            entradas.Add(new DemandaTablaDto
            {
                mes = DateTime.Now.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                cantidad = demanda
            });

            return (demanda, desviacionEstandar, entradas);
        }
        #endregion

        #region promedio movil ponderado
        public async Task<(double demanda, double desviacionEstandarPeriodo, List<DemandaTablaDto> entradas)> CalcDemandaPromedioMovilPonderado(Articulo articulo, long periodo)
        {
            int n = periodo switch
            {
                1 => 3,
                2 => 6,
                3 => 12,
                _ => throw new ArgumentException("el periodo ingresado no es válido")
            };
            await ValidarExistenciaVentas(articulo);
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);

            var ventasAgrupadas = detalles
            .Where(dVent => dVent.venta != null && dVent.venta.fechaVenta < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            .GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
            .OrderByDescending(gr => new DateTime(gr.Key.Year, gr.Key.Month, 1))
            .Take(n)
            .Reverse()
            .Select(gr => new
            {
                Fecha = new DateTime(gr.Key.Year, gr.Key.Month, 1),
                Cantidad = gr.Sum(x => x.cantidad)
            })
            .ToList();
            if (ventasAgrupadas.Count < n)  throw new Exception($"no hay suficientes datos de ventas para calcular promedio movil ponderado de {n} meses para el articulo '{articulo.nombreArticulo}'");

            var entradas = ventasAgrupadas
            .Select(vent => new DemandaTablaDto
            {
                mes = vent.Fecha.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                cantidad = vent.Cantidad
            })
            .ToList();

            //  cantidades para calculos
            var ventasPorMes = ventasAgrupadas.Select(vent => vent.Cantidad).ToList();

            // medidas de ponderacion crecientes
            var c_i = Enumerable.Range(1, n).ToList();
            double sumaC_i = c_i.Sum();

            // promedio movil ponderado
            double mediaPonderada = ventasPorMes
            .Select((x, i) => x * c_i[i])
            .Sum() / sumaC_i;

            // desviacion estandar ponderada
            double varianzaPonderada = ventasPorMes
            .Select((x, i) => c_i[i] * Math.Pow(x - mediaPonderada, 2))
            .Sum() / sumaC_i;
            double desviacion = Math.Sqrt(varianzaPonderada);

            entradas.Add(new DemandaTablaDto
            {
                mes = DateTime.Now.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                cantidad = mediaPonderada
            });
            return (mediaPonderada, desviacion, entradas);
        }
        #endregion

        #region suavizacion exponencial
        public async Task<(double demanda, double desviacionEstandar,List<DemandaTablaDto> entradas)> CalcDemandaSuavizacionExp(Articulo articulo, long periodo, double alfa)
        {
            if (alfa <= 0 || alfa >= 1) throw new ArgumentException("el parametro `alfa` debe debe estar en el rango: (0 < alfa < 1)");
            int n = periodo switch
            {
                1 => 3,
                2 => 6,
                3 => 12,
                _ => throw new ArgumentException("el periodo ingresado no es válido")
            };
            await ValidarExistenciaVentas(articulo);

            var fechaReferencia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
	        var fechaInicio = fechaReferencia.AddMonths(-n);
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);
            var ventasPorMes = detalles
            .Where(dVent => dVent.venta != null && dVent.venta.fechaVenta >= fechaInicio && dVent.venta.fechaVenta < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            .GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
            .OrderBy(gr => new DateTime(gr.Key.Year, gr.Key.Month, 1))
            .Select(gr => gr.Sum(x => x.cantidad))
            .ToList();
            if (ventasPorMes.Count < 1) throw new InvalidOperationException($"no hay suficientes ventas en los ultimos {n} meses para el artículo '{articulo.nombreArticulo}' ");

            var entradas = new List<DemandaTablaDto>();
            var ventasPorMesAgrupadas = detalles
            .Where(dVent => dVent.venta != null && dVent.venta.fechaVenta >= fechaInicio && dVent.venta.fechaVenta < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            .GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
            .OrderBy(gr => new DateTime(gr.Key.Year, gr.Key.Month, 1))
            .Select(gr => new {
                Fecha = new DateTime(gr.Key.Year, gr.Key.Month, 1),
                Cantidad = gr.Sum(x => x.cantidad)
            })
            .ToList();

            foreach (var venta in ventasPorMesAgrupadas)
            {
                entradas.Add(new DemandaTablaDto {
                    mes = venta.Fecha.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    cantidad = venta.Cantidad
                });
            }

            // inicializacion con la 1ra demanda real
            double suavizadoAnterior = ventasPorMes[0];
            var err = new List<double>();
            for (int i = 1; i < ventasPorMes.Count; i++)
            {
                double actual = ventasPorMes[i];
                double suavizado = alfa * actual + (1 - alfa) * suavizadoAnterior;
                err.Add(actual - suavizado);
                suavizadoAnterior = suavizado;
            }
            // ultimo valor suavizado
            double demanda = suavizadoAnterior;

            entradas.Add(new DemandaTablaDto {
                mes = DateTime.Now.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                cantidad = demanda
            });

            double desviacionEstandar = err.Count >= 2 ? Math.Sqrt(err.Select(e => Math.Pow(e, 2)).Sum() / err.Count) : 0;
            return (demanda, desviacionEstandar,entradas);
        }
        #endregion

        #region regresion lineal
	public async Task<(double demandaPredicha, double desviacionTotal, double desviacionExplicada, double coefCorrelacion, List<DemandaPuntoXYDto> puntos)> CalcDemandaRegLineal(Articulo articulo, long periodo)
	{
	    int n = periodo switch
	    {
		1 => 3,   
		2 => 6,   
		3 => 12, 
		_ => throw new ArgumentException("El periodo ingresado no es válido")
	    };
	    await ValidarExistenciaVentas(articulo);

	    // 1er dia del mes actual
	    var fechaReferencia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
	    var fechaInicio = fechaReferencia.AddMonths(-n);
	    var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);
	    var ventasPorMes = detalles
		.Where(dVent => dVent.venta != null &&
		                dVent.venta.fechaVenta >= fechaInicio &&
		                dVent.venta.fechaVenta < fechaReferencia)
		.GroupBy(dVent => new { dVent.venta.fechaVenta.Year, dVent.venta.fechaVenta.Month })
		.OrderBy(gr => new DateTime(gr.Key.Year, gr.Key.Month, 1))
		.Select(gr => gr.Sum(x => x.cantidad))
		.ToList();
	    if (ventasPorMes.Count < n) throw new Exception($"No hay suficientes datos de ventas para aplicar regresión lineal de {n} meses al artículo '{articulo.nombreArticulo}'.");

	    // datos x-y para regresion
	    var x = Enumerable.Range(1, ventasPorMes.Count).Select(i => (double)i).ToList();
	    var y = ventasPorMes.Select(vent => (double)vent).ToList();
	    double sumX = x.Sum();
	    double sumY = y.Sum();
	    double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
	    double sumX2 = x.Select(xi => xi * xi).Sum();
	    double x_r = sumX / n;
	    double y_r = sumY / n;

	    // coeficientes `a` y `b`
	    double b = (sumXY - n * x_r * y_r) / (sumX2 - n * x_r * x_r);
	    double a = y_r - b * x_r;

	    // demanda predicha para el prox. periodo
	    double siguienteMes = x.Last() + 1;
	    double demanda = a + b * siguienteMes;

	    // varianzas (total y explicada)
	    double s2rr = y.Count > 1 ? y.Select(yi => Math.Pow(yi - y_r, 2)).Sum() / (n - 1) : 0;
	    var yEstimada = x.Select(xi => a + b * xi).ToList();
	    double s2rc = yEstimada.Select(y_ci => Math.Pow(y_ci - y_r, 2)).Sum() / n;

	    // coeficiente de correlacion (r0)
	    double r0 = s2rr != 0 ? s2rc / s2rr : 0;

	    // puntos para graficar
	    var puntos = new List<DemandaPuntoXYDto>();
	    for (int i = 0; i < n; i++)
	    {
		puntos.Add(new DemandaPuntoXYDto
		{
		    x = i + 1,
		    yReal = y[i],
		    yEstimado = yEstimada[i]
		});
	    }

	    puntos.Add(new DemandaPuntoXYDto
	    {
		x = n + 1,
		yReal = null,
		yEstimado = demanda
	    });
	    return (demanda, s2rr, s2rc, r0, puntos);
	}
        #endregion    

        private async Task ValidarExistenciaVentas(Articulo articulo)
        {
            var detalles = await _detVentasRepository.GetByArticuloIdAsync(articulo.idArticulo);
            if (detalles == null || !detalles.Any(detV => detV.venta != null && detV.cantidad > 0)) throw new InvalidOperationException($"no existen ventas validas para el articulo '{articulo.nombreArticulo}' (Id {articulo.idArticulo})");
        }
    }

}
