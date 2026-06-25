using System.Collections.ObjectModel;
using itmodd.Models;
using itmodd.Services;
using Microsoft.Maui.Dispatching;

namespace itmodd.ViewModels;

// одна строка истории сессий
public class SessionHistoryItem
{
    public string SubjectName { get; init; } = string.Empty;
    public string Color { get; init; } = "#808080";
    public string DateText { get; init; } = string.Empty;
    public string DurationText { get; init; } = string.Empty;
}

public class TimerViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IDispatcherTimer _timer;

    // учёт времени: накопленное + момент старта текущего отрезка (локальное время)
    private TimeSpan _accumulated;
    private DateTime? _runningSince;
    private SessionApiResponse? _current;

    private readonly Dictionary<int, SubjectApiResponse> _subjectById = new();

    public TimerViewModel(IApiClient api)
    {
        _api = api;

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, _) => OnPropertyChanged(nameof(ElapsedText));

        StartCommand = new Command(async () => await StartAsync(), () => CanStart);
        PauseCommand = new Command(async () => await PauseAsync());
        ResumeCommand = new Command(async () => await ResumeAsync());
        CompleteCommand = new Command(async () => await CompleteAsync());
    }

    // ===== данные =====
    public ObservableCollection<SubjectApiResponse> Subjects { get; } = new();
    public ObservableCollection<SessionHistoryItem> History { get; } = new();

    private SubjectApiResponse? _selectedSubject;
    public SubjectApiResponse? SelectedSubject
    {
        get => _selectedSubject;
        set { _selectedSubject = value; OnPropertyChanged(); ((Command)StartCommand).ChangeCanExecute(); }
    }

    // ===== состояние =====
    // нет активной/приостановленной сессии -> можно выбирать предмет и стартовать
    public bool IsIdle => _current is null;
    public bool IsRunning => _current is not null && _runningSince is not null;
    public bool IsPaused => _current is not null && _runningSince is null;
    public bool CanStart => IsIdle && SelectedSubject is not null;

    // подпись текущего предмета
    public string CurrentSubjectName =>
        _current is not null && _subjectById.TryGetValue(_current.SubjectId, out var s) ? s.SubjectName : string.Empty;

    public string ElapsedText
    {
        get
        {
            var total = _accumulated + (_runningSince is { } t ? DateTime.Now - t : TimeSpan.Zero);
            if (total < TimeSpan.Zero) total = TimeSpan.Zero;
            return $"{(int)total.TotalHours:D2}:{total.Minutes:D2}:{total.Seconds:D2}";
        }
    }

    private string _error = string.Empty;
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(_error);

    public Command StartCommand { get; }
    public Command PauseCommand { get; }
    public Command ResumeCommand { get; }
    public Command CompleteCommand { get; }

    // ===== загрузка/восстановление =====
    public async Task InitAsync()
    {
        var subjects = await _api.GetSubjectsAsync();
        _subjectById.Clear();
        Subjects.Clear();
        foreach (var s in subjects.Where(s => !s.IsArchived))
        {
            Subjects.Add(s);
            _subjectById[s.SubjectId] = s;
        }
        SelectedSubject ??= Subjects.FirstOrDefault();

        var sessions = await _api.GetSessionsAsync();

        // восстановить активную/приостановленную сессию
        var ongoing = sessions.FirstOrDefault(s => s.IsActive) ?? sessions.FirstOrDefault(s => s.IsPaused);
        if (ongoing is not null)
        {
            _current = ongoing;
            // приблизительно: время от старта (паузы до перезапуска не вычитаем)
            _accumulated = DateTime.UtcNow - ongoing.StartedAt.ToUniversalTime();
            if (_accumulated < TimeSpan.Zero) _accumulated = TimeSpan.Zero;

            if (ongoing.IsActive) { _runningSince = DateTime.Now; _timer.Start(); }
            else _runningSince = null; // на паузе — счётчик стоит
        }

        BuildHistory(sessions);
        RaiseStateChanged();
    }

    private void BuildHistory(IReadOnlyList<SessionApiResponse> sessions)
    {
        History.Clear();
        foreach (var s in sessions.Where(s => s.EndedAt is not null).OrderByDescending(s => s.StartedAt))
        {
            var name = _subjectById.TryGetValue(s.SubjectId, out var subj) ? subj.SubjectName : "Предмет";
            var color = subj?.Color ?? "#808080";
            var dur = s.EndedAt!.Value - s.StartedAt;
            if (dur < TimeSpan.Zero) dur = TimeSpan.Zero;

            History.Add(new SessionHistoryItem
            {
                SubjectName = name,
                Color = color,
                DateText = s.StartedAt.ToLocalTime().ToString("d MMM, HH:mm"),
                DurationText = $"{(int)dur.TotalHours:D2}:{dur.Minutes:D2}:{dur.Seconds:D2}",
            });
        }
    }

    // ===== действия =====
    private async Task StartAsync()
    {
        if (SelectedSubject is null) return;
        Error = string.Empty;

        var (session, error) = await _api.StartSessionAsync(SelectedSubject.SubjectId);
        if (session is null)
        {
            Error = error ?? "Не удалось начать сессию";
            return;
        }

        _current = session;
        _accumulated = TimeSpan.Zero;
        _runningSince = DateTime.Now;
        _timer.Start();
        RaiseStateChanged();
    }

    private async Task PauseAsync()
    {
        if (_current is null || _runningSince is null) return;

        var res = await _api.PauseSessionAsync(_current.Id);
        if (res is null) { Error = "Не удалось поставить на паузу"; return; }

        _accumulated += DateTime.Now - _runningSince.Value;
        _runningSince = null;
        _timer.Stop();
        RaiseStateChanged();
    }

    private async Task ResumeAsync()
    {
        if (_current is null || _runningSince is not null) return;

        var res = await _api.ResumeSessionAsync(_current.Id);
        if (res is null) { Error = "Не удалось продолжить"; return; }

        _runningSince = DateTime.Now;
        _timer.Start();
        RaiseStateChanged();
    }

    private async Task CompleteAsync()
    {
        if (_current is null) return;

        var res = await _api.CompleteSessionAsync(_current.Id);
        if (res is null) { Error = "Не удалось завершить"; return; }

        _timer.Stop();
        _current = null;
        _accumulated = TimeSpan.Zero;
        _runningSince = null;
        Error = string.Empty;

        // перечитать историю
        BuildHistory(await _api.GetSessionsAsync());
        RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        OnPropertyChanged(nameof(IsIdle));
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsPaused));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CurrentSubjectName));
        OnPropertyChanged(nameof(ElapsedText));
        ((Command)StartCommand).ChangeCanExecute();
    }
}
