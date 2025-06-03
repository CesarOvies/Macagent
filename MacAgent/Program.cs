using MacAgent.Handlers;
#if !DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#endif

namespace MacAgent;

public class Program
{
    public static async Task Main()
    {
        try
        {
#if DEBUG
            await new ComputerInventoryHandler().Executa();
#else
            IHostBuilder builder = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ServiceWorker>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}