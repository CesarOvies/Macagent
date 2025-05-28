using Mac.Interfaces;
namespace Mac.Handlers;

public class InventariaComputadorHandler : IInventarioHandler
{
    public string Nome => "Computador";

    public Task Executa()
    {
        string nome = System.Net.Dns.GetHostName();
        string os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        Console.WriteLine($"Nome computador: {nome} \n Versão OS: {os}");

        return Task.CompletedTask;
    }
}