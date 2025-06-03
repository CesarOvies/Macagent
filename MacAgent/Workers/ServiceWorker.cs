using System.Timers;
using MacAgent.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace MacAgent.Handlers;

public class ServiceWorker : BackgroundService
{
    private readonly Timer _timer;
    private readonly List<IInventarioHandler> handlers;
    private readonly Dictionary<string, bool> handlers_controller;
    private readonly ILogger<ServiceWorker> logger;

    public ServiceWorker(ILogger<ServiceWorker> logger)
    {
        this.logger = logger;
        
        handlers = new List<IInventarioHandler>
        {
            new ComputerInventoryHandler(),
        };

        handlers_controller = handlers.ToDictionary(x => x.Nome, _ => false);

        _timer = new Timer
        {
            Interval = TimeSpan.FromMinutes(5).TotalMilliseconds,
            AutoReset = true
        };
        _timer.Elapsed += OnTimerElapsed;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Serviço de inventário iniciado.");
        _timer.Start();
        
        await ExecutarTodosAsync(stoppingToken);

        while (stoppingToken.IsCancellationRequested == false)
        {
            await Task.Delay(1000, stoppingToken);
        }
        
        _timer.Stop();
        logger.LogInformation("Serviço de inventário parado.");
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await ExecutarTodosAsync(default);
    }

    private async Task ExecutarTodosAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(handlers.Select(h => ExecutarHandlerAsync(h, cancellationToken)));
    }

    private async Task ExecutarHandlerAsync(IInventarioHandler handler, CancellationToken cancellationToken)
    {
        if (handlers_controller[handler.Nome] ||
        cancellationToken.IsCancellationRequested)
        {
            return;
        }

        handlers_controller[handler.Nome] = true;

        try
        {
            logger.LogInformation("Executando handler: {HandlerNome}", handler.Nome);
            await handler.Executa();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao executar {HandlerNome}", handler.Nome);
        }
        finally
        {
            handlers_controller[handler.Nome] = false;
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}