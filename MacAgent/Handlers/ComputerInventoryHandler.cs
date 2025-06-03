using MacAgent.Interfaces;

namespace MacAgent.Handlers;

public class ComputerInventoryHandler : IInventarioHandler
{
    public string Nome => "Computador";

    public Task Executa()
    {
        // string modelo = DeviceInfo.Model;
        // string nome = DeviceInfo.Name;
        // var sub_tipo = DeviceInfo.Platform.GetType();

        // Console.WriteLine($"Nome computador: {modelo} \n Versão OS: {nome}");

        return Task.CompletedTask;
    }
}