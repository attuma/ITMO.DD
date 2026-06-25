using System.Collections.ObjectModel;
using itmodd.Models;
using itmodd.Services;
using Microsoft.Maui.Dispatching;

namespace itmodd.ViewModels;

/// <summary>Готовая к показу строка истории завершённых сессий (нижний список на экране таймера).</summary>
public class SessionHistoryItem
{
    /// <summary>Название предмета.</summary>
    public string SubjectName { get; init; } = string.Empty;

    /// <summary>Цвет предмета (hex) — для цветной метки в списке.</summary>
    public string Color { get; init; } = "#808080";

    /// <summary>Когда была сессия, напр. «5 июн, 14:30».</summary>
    public string DateText { get; init; } = string.Empty;

    /// <summary>Длительность «ЧЧ:ММ:СС».</summary>
    public string DurationText { get; init; } = string.Empty;
}

/// <summary>
/// ViewModel экрана «Таймер». Управляет одной активной сессией
/// (старт/пауза/продолжить/завершить через API), тикающим счётчиком на экране
/// и историей завершённых сессий снизу.
/// </summary>
public class TimerViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IDispatcherTimer _timer;

    // Учёт времени держим локально, чтобы счётчик тикал плавно без дёрганья сервера:
    //  _accumulated  — сколько уже «накапали» до текущего запущенного отрезка;
    //  _runningSince — момент старта текущего отрезка (null = на паузе/стоит).
    // Итого на экране = _accumulated + (сейчас - _runningSince).
    private TimeSpan _accumulated;
    private DateTime? _runningSince;
    private SessionApiResponse? _current;   // текущая сессия с сервера (null = простой)

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

    // ===== состояние (бинды кнопок/панелей в XAML) =====

    /// <summary>Нет сессии — можно выбрать предмет и нажать «Старт».</summary>
    public bool IsIdle => _current is null;

    /// <summary>Сессия идёт (есть текущая и отсчёт запущен).</summary>
    public bool IsRunning => _current is not null && _runningSince is not null;

    /// <summary>Сессия есть, но на паузе (отсчёт остановлен).</summary>
    public bool IsPaused => _current is not null && _runningSince is null;

    /// <summary>Кнопка «Старт» активна только в простое и при выбранном предмете.</summary>
    public bool CanStart => IsIdle && SelectedSubject is not null;

    // подпись текущего предмета
    public string CurrentSubjectName =>
        _current is not null && _subjectById.TryGetValue(_current.SubjectId, out var s) ? s.SubjectName : string.Empty;

    /// <summary>Прошедшее время для экрана в формате «ЧЧ:ММ:СС». Пересчитывается каждую секунду по тику таймера.</summary>
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

    /// <summary>
    /// Вызывается при открытии экрана. Грузит предметы и сессии, и если на сервере
    /// есть незакрытая сессия (active/paused) — восстанавливает её, чтобы таймер
    /// «продолжился» после перезапуска приложения. Затем строит историю.
    /// </summary>
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

    /// <summary>
    /// Собирает нижний список истории из завершённых сессий (у которых есть EndedAt),
    /// от новых к старым. Длительность = EndedAt − StartedAt (пауз не вычитаем — приближение).
    /// </summary>
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

    // ===== действия (кнопки) =====

    /// <summary>«Старт»: создаёт сессию на сервере, обнуляет счётчик и запускает тик.</summary>
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

    /// <summary>«Пауза»: фиксирует накопленное время в _accumulated, останавливает тик.</summary>
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

    /// <summary>«Продолжить»: снова запоминает момент старта отрезка и включает тик.</summary>
    private async Task ResumeAsync()
    {
        if (_current is null || _runningSince is not null) return;

        var res = await _api.ResumeSessionAsync(_current.Id);
        if (res is null) { Error = "Не удалось продолжить"; return; }

        _runningSince = DateTime.Now;
        _timer.Start();
        RaiseStateChanged();
    }

    /// <summary>«Завершить»: закрывает сессию на сервере, сбрасывает состояние и перечитывает историю.</summary>
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

    /// <summary>Дёргает все вычисляемые состояния разом, чтобы UI (кнопки/панели) перерисовался.</summary>
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
