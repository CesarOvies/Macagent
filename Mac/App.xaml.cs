using Mac.Handlers;
using Mac.Pages;

namespace Mac;

public partial class App : Application
{
	private readonly IniciaServico servico;

	public App()
	{
		InitializeComponent();
		servico = new IniciaServico();
	}

	protected override void OnStart()
	{
		servico.Iniciar();
	}
}
