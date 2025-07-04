using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Models;

namespace Proyect_InvOperativa.Services
{
    public class ControlStockPeriodoFijoService : BackgroundService
    {
        private readonly ILogger<ControlStockPeriodoFijoService> _logger;
        private readonly MaestroArticulosService _maestroArticuloService;
        private readonly TimeSpan _intervalo;

        public ControlStockPeriodoFijoService(
            ILogger<ControlStockPeriodoFijoService> logger,
            MaestroArticulosService maestroArticuloService)
        {
            _logger = logger;
            _maestroArticuloService = maestroArticuloService;
            _intervalo = TimeSpan.FromMinutes(60); // si se quiere en horas o dias usar FromHours / FromDays
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ControlStockPeriodoFijoService iniciado ");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _maestroArticuloService.ControlStockPeriodico(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "error al ejecutar ControlStock periodico ");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
            _logger.LogInformation("controlStockPeriodoFijoService detenido ");
        }
    }
}
