namespace MacAgent.Interfaces;

public interface IInventarioHandler
{
    string Nome { get; }
    Task Executa();
}