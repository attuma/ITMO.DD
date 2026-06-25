using Microsoft.Extensions.DependencyInjection;

namespace itmodd;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// стартуем с экрана входа; он сам перейдёт в AppShell при наличии токена
		return new Window(new Views.LoginPage());
	}
}