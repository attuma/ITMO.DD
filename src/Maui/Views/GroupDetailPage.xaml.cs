using itmodd.ViewModels;

namespace itmodd.Views;

public partial class GroupDetailPage : ContentPage
{
    private readonly GroupDetailViewModel _vm;

    public GroupDetailPage(GroupDetailViewModel vm)
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
