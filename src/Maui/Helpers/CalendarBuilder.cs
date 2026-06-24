using itmodd.Models;

namespace itmodd.Helpers;

// Строит сетку календаря: из месяца/года делает 42 ячейки (6 недель x 7 дней).
// Не ходит в БД и не рисует — только даты.
public class CalendarBuilder
{

    public List<DayCell> BuildMonth(int year, int month)
    {
        var cells = new List<DayCell>();

        // Шаг 1: первое число месяца
        var firstDay = new DateTime(year, month, 1);

        // Шаг 2: на сколько дней отступить назад до понедельника.
        // В C# DayOfWeek: Воскресенье=0, Понедельник=1, ... Суббота=6.
        // Нам нужно ПН=0 ... ВС=6. Формула ниже это и делает:
        //   у понедельника (1) -> 0
        //   у воскресенья (0) -> 6
        int offset = ((int)firstDay.DayOfWeek + 6) % 7;

        // отступаем назад -> это понедельник той недели, где лежит 1-е число
        var start = firstDay.AddDays(-offset);

        // Шаг 3: ровно 42 ячейки (6 недель x 7 дней)
        for (int i = 0; i < 42; i++)
        {
            var date = start.AddDays(i);

            // Шаг 4: флаги
            cells.Add(new DayCell
            {
                Date = date,
                IsCurrentMonth = date.Month == month,        // из этого месяца?
                IsToday = date.Date == DateTime.Today         // сегодня?
            });
        }

        return cells;
    }
}
