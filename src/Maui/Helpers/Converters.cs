using System.Globalization;

namespace itmodd.Helpers;

// Превращает hex-строку ("#FF8C00") в Color для точки предмета.
// Если строка пустая или кривая — отдаём серый, чтобы UI не падал.
public class StringToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hex = value as string;
        if (string.IsNullOrWhiteSpace(hex))
            return Colors.Gray;

        try { return Color.FromArgb(hex); }
        catch { return Colors.Gray; }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// bool -> прозрачность. true (день этого месяца) = 1.0, false (соседний) = бледный.
public class BoolToOpacityConverter : IValueConverter
{
    public double TrueOpacity { get; set; } = 1.0;
    public double FalseOpacity { get; set; } = 0.35;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? TrueOpacity : FalseOpacity;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
