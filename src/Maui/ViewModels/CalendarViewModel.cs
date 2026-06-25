using System.Collections.ObjectModel;
using itmodd.Helpers;
using itmodd.Models;
using itmodd.Services;

namespace itmodd.ViewModels;

public class CalendarViewModel : BaseViewModel
{
    // повар создаём один раз и пользуемся
    private readonly CalendarBuilder _builder = new();

    // источник задач (заглушка или API) — VM не знает, что именно
    private readonly ICalendarDataService _data;

    // какой месяц сейчас показываем храним 1-е число этого месяца
    private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    // ячейки, которые видит экран
    public ObservableCollection<DayCell> Days { get; } = new();

    // счётчики верхних карточек
    private int _todayCount, _weekCount, _overdueCount, _doneCount;
    public int TodayCount   { get => _todayCount;   private set { _todayCount = value;   OnPropertyChanged(); OnPropertyChanged(nameof(TodayText)); } }
    public int WeekCount    { get => _weekCount;    private set { _weekCount = value;    OnPropertyChanged(); OnPropertyChanged(nameof(WeekText)); } }
    public int OverdueCount { get => _overdueCount; private set { _overdueCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(OverdueText)); } }
    public int DoneCount    { get => _doneCount;    private set { _doneCount = value;    OnPropertyChanged(); OnPropertyChanged(nameof(DoneText)); } }

    // тексты со склонением: «3 задачи», «8 задач», «1 задача»
    public string TodayText   => Plural(_todayCount);
    public string WeekText    => Plural(_weekCount);
    public string OverdueText => Plural(_overdueCount);
    public string DoneText    => Plural(_doneCount);

    // русское склонение слова «задача» по числу
    private static string Plural(int n)
    {
        int mod100 = n % 100, mod10 = n % 10;
        string word =
            (mod100 is >= 11 and <= 14) ? "задач"
            : mod10 == 1 ? "задача"
            : (mod10 is >= 2 and <= 4) ? "задачи"
            : "задач";
        return $"{n} {word}";
    }

    // заголовок вверху, например "June 2026"
    private string _monthTitle = string.Empty;
    public string MonthTitle
    {
        get => _monthTitle;
        set { _monthTitle = value; OnPropertyChanged(); }
    }

    // кнопки ‹ ›
    public Command PreviousMonthCommand { get; }
    public Command NextMonthCommand { get; }

    // тап по дню
    public Command<DayCell> SelectDayCommand { get; }

    // выбранный день (для нижнего списка master-detail)
    private DayCell? _selectedDay;
    public DayCell? SelectedDay
    {
        get => _selectedDay;
        private set
        {
            _selectedDay = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedDay));
            OnPropertyChanged(nameof(SelectedDayTitle));
            OnPropertyChanged(nameof(SelectedDayTasks));
        }
    }

    // показывать ли нижнюю панель
    public bool HasSelectedDay => _selectedDay is not null;

    // заголовок панели, например "25 июня 2026"
    public string SelectedDayTitle => _selectedDay?.Date.ToString("d MMMM yyyy") ?? string.Empty;

    // задачи выбранного дня (все, не только видимые в ячейке)
    public IReadOnlyList<DeadlineItem> SelectedDayTasks =>
        _selectedDay?.Tasks ?? (IReadOnlyList<DeadlineItem>)Array.Empty<DeadlineItem>();

    public CalendarViewModel(ICalendarDataService data)
    {
        _data = data;
        PreviousMonthCommand = new Command(GoToPreviousMonth);
        NextMonthCommand = new Command(GoToNextMonth);
        SelectDayCommand = new Command<DayCell>(SelectDay);
        _ = LoadMonthAsync();    // сразу показать текущий месяц при открытии
        _ = LoadSummaryAsync();  // и заполнить счётчики
    }

    // перечитать данные (после создания задачи): текущий месяц + счётчики
    public async Task RefreshAsync()
    {
        await LoadMonthAsync();
        await LoadSummaryAsync();
    }

    // счётчики не зависят от показанного месяца — грузим один раз
    private async Task LoadSummaryAsync()
    {
        var s = await _data.GetSummaryAsync();
        TodayCount = s.Today;
        WeekCount = s.Week;
        OverdueCount = s.Overdue;
        DoneCount = s.Done;
    }

    // выбрать день: снять прошлое выделение, поставить новое
    private void SelectDay(DayCell? cell)
    {
        if (cell is null)
            return;

        if (_selectedDay is not null)
            _selectedDay.IsSelected = false;

        cell.IsSelected = true;
        SelectedDay = cell;
    }

    // главный метод: построить сетку, спросить у сервиса задачи и разложить по дням
    private async Task LoadMonthAsync()
    {
        SelectedDay = null;   // выделение прошлого месяца больше не актуально
        MonthTitle = _currentMonth.ToString("MMMM yyyy");

        var cells = _builder.BuildMonth(_currentMonth.Year, _currentMonth.Month);

        // задачи месяца от сервиса (сейчас заглушка, позже API)
        var deadlines = await _data.GetDeadlinesAsync(_currentMonth.Year, _currentMonth.Month);
        foreach (var task in deadlines)
        {
            // ищем ячейку, у которой дата совпадает с дедлайном задачи
            var cell = cells.FirstOrDefault(c => c.Date.Date == task.Deadline.Date);
            cell?.Tasks.Add(task);
        }

        Days.Clear();
        foreach (var cell in cells)
            Days.Add(cell);
    }

    private async void GoToPreviousMonth()
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        await LoadMonthAsync();
    }

    private async void GoToNextMonth()
    {
        _currentMonth = _currentMonth.AddMonths(1);
        await LoadMonthAsync();
    }
}