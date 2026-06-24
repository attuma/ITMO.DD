using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace itmodd.Models;

// Одна ячейка календаря (один день).
public class DayCell : INotifyPropertyChanged
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public List<DeadlineItem> Tasks { get; set; } = new();

    // Выделена ли ячейка тапом. Меняется в рантайме -> с уведомлением.
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Сколько задач показываем текстом в ячейке (остальные сворачиваем в бейдж).
    private const int MaxVisible = 2;

    // Первые 1–2 задачи — их рисуем построчно.
    public IEnumerable<DeadlineItem> VisibleTasks => Tasks.Take(MaxVisible);

    // Сколько задач не влезло.
    public int ExtraCount => Math.Max(0, Tasks.Count - MaxVisible);

    // Показывать ли бейдж «+N задача».
    public bool HasExtra => ExtraCount > 0;

    // Текст бейджа.
    public string ExtraText => $"+{ExtraCount} задача";

    // Есть ли вообще задачи в этом дне.
    public bool HasTasks => Tasks.Count > 0;

    // Пустой день текущего месяца -> показываем «Задач нет».
    public bool IsEmptyDay => IsCurrentMonth && Tasks.Count == 0;

    // День недели сокращённо: ПН..ВС (ПН=0).
    private static readonly string[] WeekdaysShort =
        { "ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ", "ВС" };
    public string WeekdayShort => WeekdaysShort[((int)Date.DayOfWeek + 6) % 7];

    // Месяц в родительном падеже: «июня», «июля» (как «16 июня»).
    private static readonly string[] MonthsGenitive =
    {
        "января", "февраля", "марта", "апреля", "мая", "июня",
        "июля", "августа", "сентября", "октября", "ноября", "декабря"
    };
    public string MonthGenitive => MonthsGenitive[Date.Month - 1];
}

