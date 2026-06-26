using System.Collections.ObjectModel;
using itmodd.Models;
using itmodd.Services;
using itmodd.Views;

namespace itmodd.ViewModels;

/// <summary>
/// ViewModel экрана «Группы». Показывает группы пользователя и позволяет вступить по коду.
/// </summary>
public class GroupsViewModel : BaseViewModel
{
    private readonly IApiClient _api;

    public GroupsViewModel(IApiClient api)
    {
        _api = api;
        JoinCommand = new Command(async () => await JoinAsync(), () => !IsBusy);
        CreateCommand = new Command(async () => await CreateAsync(), () => !IsBusy);
        OpenGroupCommand = new Command<GroupApiResponse>(async g => await OpenGroupAsync(g));
    }

    public ObservableCollection<GroupApiResponse> Groups { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((Command)JoinCommand).ChangeCanExecute(); ((Command)CreateCommand).ChangeCanExecute(); }
    }

    private string _joinCode = string.Empty;
    public string JoinCode
    {
        get => _joinCode;
        set { _joinCode = value; OnPropertyChanged(); }
    }

    private string _error = string.Empty;
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(_error);

    public Command JoinCommand { get; }
    public Command CreateCommand { get; }
    public Command<GroupApiResponse> OpenGroupCommand { get; }

    public async Task InitAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _api.GetGroupsAsync();
            Groups.Clear();
            foreach (var g in list)
                Groups.Add(g);
        }
        finally { IsBusy = false; }
    }

    private async Task JoinAsync()
    {
        if (string.IsNullOrWhiteSpace(JoinCode)) return;
        Error = string.Empty;
        IsBusy = true;
        try
        {
            var (group, error) = await _api.JoinGroupAsync(JoinCode.Trim().ToUpper());
            if (group == null)
            {
                Error = error ?? "Группа не найдена";
                return;
            }
            JoinCode = string.Empty;
            Groups.Add(group);
        }
        finally { IsBusy = false; }
    }

    private async Task CreateAsync()
    {
        var name = await Shell.Current.DisplayPromptAsync(
            "Новая группа",
            "Введи название группы",
            accept: "Создать",
            cancel: "Отмена",
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(name)) return;
        Error = string.Empty;
        IsBusy = true;
        try
        {
            var (group, error) = await _api.CreateGroupAsync(name.Trim());
            if (group == null)
            {
                Error = error ?? "Не удалось создать группу";
                return;
            }
            Groups.Add(group);
        }
        finally { IsBusy = false; }
    }

    private async Task OpenGroupAsync(GroupApiResponse group)
    {
        await Shell.Current.GoToAsync($"GroupDetailPage?groupId={group.Id}&groupName={Uri.EscapeDataString(group.GroupName)}&joinCode={group.JoinCode}");
    }
}
