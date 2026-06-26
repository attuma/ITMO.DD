namespace itmodd.Services;

// Настройки доступа к API.
public static class ApiConfig
{
    // адрес API. localhost подходит для MacCatalyst/Windows.
    // (для Android-эмулятора будет 10.0.2.2, для реального устройства — IP машины)
    public const string BaseUrl = "http://138.16.168.170:5000";
}
