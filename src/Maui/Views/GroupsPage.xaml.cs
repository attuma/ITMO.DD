using itmodd.ViewModels;

namespace itmodd.Views;

public partial class GroupsPage : ContentPage
{
    private readonly GroupsViewModel _vm;

    public GroupsPage(GroupsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }
}
