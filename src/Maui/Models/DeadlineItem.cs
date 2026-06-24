namespace itmodd.Models;

// Одна задача в том виде, в каком её рисует календарь (UI-модель).
// Это НЕ доменный TaskItem — здесь только то, что нужно показать в ячейке.
public class DeadlineItem
{
    public string Title { get; set; } = string.Empty;  // название задачи
    public DateTime Deadline { get; set; }              // когда дедлайн (дата + время)
    public string Color { get; set; } = "#808080";      // цвет точки (= предмет), пока серый
    public bool IsDone { get; set; }                    // выполнена ли

    // Просрочено: дедлайн в прошлом И задача ещё не выполнена.
    public bool IsOverdue => !IsDone && Deadline < DateTime.Now;
}
