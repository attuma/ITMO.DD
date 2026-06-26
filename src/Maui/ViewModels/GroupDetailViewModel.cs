using System.Collections.ObjectModel;
using itmodd.Models;
using itmodd.Services;

namespace itmodd.ViewModels;


/// <summary>Строка участника группы готовая к показу в CollectionView.</summary>
public class MemberRow
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public bool IsLeader { get; init; }
    public bool IsStudying { get; init; }
    public string TodayTimeText { get; init; } = string.Empty;
    public string LeaderBadge => IsLeader ? "👑" : string.Empty;
    public string StudyingBadge => IsStudying ? "🟢" : "⚪";
}

/// <summary>
/// ViewModel экрана «Участники группы». Показывает всех участников с их текущим
/// статусом учёбы и временем за сегодня. Лидер группы выделен особо.
/// </summary>
[QueryProperty(nameof(GroupId), "groupId")]
[QueryProperty(nameof(GroupName), "groupName")]
[QueryProperty(nameof(JoinCode), "joinCode")]
public class GroupDetailViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IAuthService _auth;

    public GroupDetailViewModel(IApiClient api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
        RefreshCommand = new Command(async () => await LoadAsync());
        ArchiveCommand = new Command(async () => await ArchiveAsync());
    }

    public ObservableCollection<MemberRow> Members { get; } = new();

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set { _groupId = value; OnPropertyChanged(); }
    }

    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set { _groupName = value; OnPropertyChanged(); }
    }

    private string _joinCode = string.Empty;
    public string JoinCode
    {
        get => _joinCode;
        set { _joinCode = value; OnPropertyChanged(); OnPropertyChanged(nameof(JoinCodeText)); }
    }

    public string JoinCodeText => $"Код: {JoinCode}";

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

    private bool _isCurrentUserLeader;
    public bool IsCurrentUserLeader
    {
        get => _isCurrentUserLeader;
        set { _isCurrentUserLeader = value; OnPropertyChanged(); }
    }

    public Command RefreshCommand { get; }
    public Command ArchiveCommand { get; }

    public async Task InitAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        if (GroupId == 0) return;
        IsBusy = true;
        try
        {
            var list = await _api.GetGroupMembersAsync(GroupId);
            Members.Clear();
            // лидер идёт первым
            foreach (var m in list.OrderByDescending(m => m.GroupRole == "GroupOwner"))
            {
                Members.Add(new MemberRow
                {
                    UserId = m.UserId,
                    Username = m.Username,
                    IsLeader = m.GroupRole == "GroupOwner",
                    IsStudying = m.IsStudying,
                    TodayTimeText = FormatTime(m.TodaySeconds),
                });
            }
            IsCurrentUserLeader = Members.Any(m => m.IsLeader && m.UserId == _auth.CurrentUserId);
        }
        finally { IsBusy = false; }
    }

    private async Task ArchiveAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Удалить группу?",
            $"Группа «{GroupName}» будет архивирована. Статистика участников сохранится.",
            "Удалить",
            "Отмена");

        if (!confirmed) return;

        var ok = await _api.ArchiveGroupAsync(GroupId);
        if (ok)
            await Shell.Current.GoToAsync("..");
        else
            await Shell.Current.DisplayAlert("Ошибка", "Не удалось архивировать группу", "ОК");
    }

    private static string FormatTime(long totalSeconds)
    {
        var h = totalSeconds / 3600;
        var m = (totalSeconds % 3600) / 60;
        if (h > 0) return $"{h} ч {m} мин";
        if (m > 0) return $"{m} мин";
        return "0 мин";
    }
}
