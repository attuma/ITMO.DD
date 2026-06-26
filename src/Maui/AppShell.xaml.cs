using itmodd.Views;

namespace itmodd;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("GroupDetailPage", typeof(GroupDetailPage));
	}
}
