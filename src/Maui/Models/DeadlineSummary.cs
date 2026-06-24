namespace itmodd.Models;

// Счётчики для карточек сверху календаря.
// Считаются по всем задачам пользователя (не по показанному месяцу).
public record DeadlineSummary(int Today, int Week, int Overdue, int Done)
{
    public static DeadlineSummary Empty => new(0, 0, 0, 0);
}
