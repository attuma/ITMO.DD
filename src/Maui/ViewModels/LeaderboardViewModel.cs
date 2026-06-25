using System.Collections.ObjectModel;
using itmodd.Services;

namespace itmodd.ViewModels;

/// <summary>
/// Готовая к показу строка рейтинга (то, что биндится в CollectionView).
/// В отличие от <see cref="LeaderboardEntryResponse"/> здесь уже всё «причёсано»
/// для UI: место медалью, время текстом, флаг «это я».
/// </summary>
public class LeaderboardRow
{
    /// <summary>Место: 🥇/🥈/🥉 для топ-3, иначе «#4», «#5»…</summary>
    public string RankText { get; init; } = string.Empty;

    /// <summary>Имя пользователя.</summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>Время учёбы человекочитаемо, напр. «2 ч 15 мин».</summary>
    public string TimeText { get; init; } = string.Empty;

    /// <summary>true — это строка текущего пользователя (в XAML подсвечивается рамкой).</summary>
    public bool IsMe { get; init; }
}

/// <summary>
/// ViewModel экрана «Рейтинг». Грузит топ пользователей за выбранный период
/// (день/неделя/месяц) и превращает сырые ответы API в готовые к показу строки.
/// </summary>
public class LeaderboardViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IAuthService _auth;

    private string _period = "daily";

    public LeaderboardViewModel(IApiClient api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
        SelectPeriodCommand = new Command<string>(async p => await SetPeriodAsync(p));
    }

    public ObservableCollection<LeaderboardRow> Entries { get; } = new();

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

    // фон кнопок периода (выбранный = синий) — через StringToColor в XAML
    public string DailyBg => Bg("daily");
    public string WeeklyBg => Bg("weekly");
    public string MonthlyBg => Bg("monthly");
    private string Bg(string p) => _period == p ? "#0A84FF" : "#1C1C1E";

    public Command<string> SelectPeriodCommand { get; }

    /// <summary>Вызывается при открытии страницы (OnAppearing) — первая загрузка рейтинга.</summary>
    public async Task InitAsync() => await LoadAsync();

    /// <summary>
    /// Переключение периода кнопками «Сегодня/Неделя/Месяц». Меняет фон кнопок
    /// (выбранная синяя) и перезагружает список.
    /// </summary>
    private async Task SetPeriodAsync(string period)
    {
        _period = period;
        OnPropertyChanged(nameof(DailyBg));
        OnPropertyChanged(nameof(WeeklyBg));
        OnPropertyChanged(nameof(MonthlyBg));
        await LoadAsync();
    }

    /// <summary>
    /// Тянет рейтинг за текущий период из API и пересобирает <see cref="Entries"/>:
    /// проставляет место (медали), форматирует время и помечает строку «себя»
    /// сравнением UserId с <see cref="IAuthService.CurrentUserId"/>.
    /// </summary>
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _api.GetLeaderboardAsync(_period);
            Entries.Clear();

            var rank = 1;
            foreach (var e in list)
            {
                Entries.Add(new LeaderboardRow
                {
                    RankText = Medal(rank),
                    Username = e.Username,
                    TimeText = FormatTime(e.TotalSeconds),
                    IsMe = e.UserId == _auth.CurrentUserId,
                });
                rank++;
            }
        }
        finally { IsBusy = false; }
    }

    /// <summary>Топ-3 получают медали, остальные — «#N».</summary>
    private static string Medal(int rank) => rank switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => $"#{rank}"
    };

    /// <summary>Секунды → «2 ч 15 мин» / «15 мин» / «меньше минуты».</summary>
    private static string FormatTime(long totalSeconds)
    {
        var h = totalSeconds / 3600;
        var m = (totalSeconds % 3600) / 60;
        if (h > 0) return $"{h} ч {m} мин";
        if (m > 0) return $"{m} мин";
        return "меньше минуты";
    }
}
