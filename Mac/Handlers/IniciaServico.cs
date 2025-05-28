using System.Timers;
using Mac.Interfaces;
using Timer = System.Timers.Timer;

namespace Mac.Handlers;

public class IniciaServico
{
    private readonly Timer cronometro_execucao;
    private readonly List<IInventarioHandler> handlers;
    private readonly Dictionary<string, bool> controle_execucao_handlers;

    public IniciaServico()
    {
        handlers = new List<IInventarioHandler>
        {
            new InventariaComputadorHandler(),
        };

        controle_execucao_handlers = handlers.ToDictionary(h => h.Nome, _ => false);

        cronometro_execucao = new Timer
        {
            Interval = TimeSpan.FromMinutes(5).TotalMilliseconds,
            AutoReset = true
        };
        cronometro_execucao.Elapsed += OnTimerElapsed;
    }

    public void Iniciar()
    {
        cronometro_execucao.Start();
        _ = ExecutarTodosAsync();
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await ExecutarTodosAsync();
    }

    private async Task ExecutarTodosAsync()
    {
        await Task.WhenAll(handlers.Select(h => ExecutarHandlerAsync(h)).ToList());
    }

    private async Task ExecutarHandlerAsync(IInventarioHandler handler)
    {
        if (controle_execucao_handlers[handler.Nome])
        {
            return;
        }

        controle_execucao_handlers[handler.Nome] = true;

        try
        {
            await handler.Executa();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao executar {handler.Nome}: {ex.Message}");
        }
        finally
        {
            controle_execucao_handlers[handler.Nome] = false;
        }
    }
}
