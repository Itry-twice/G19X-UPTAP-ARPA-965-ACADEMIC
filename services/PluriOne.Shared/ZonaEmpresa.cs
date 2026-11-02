namespace PluriOne.Shared;

/// <summary>
/// Convierte fechas/horas a la zona horaria de la empresa (por omisión America/Mexico_City),
/// configurable con la variable de entorno APP_TIMEZONE.
/// </summary>
public static class ZonaEmpresa
{
    static readonly TimeZoneInfo Tz = ResolverTz();

    static TimeZoneInfo ResolverTz()
    {
        var id = Environment.GetEnvironmentVariable("APP_TIMEZONE");
        if (string.IsNullOrWhiteSpace(id))
        {
            id = "America/Mexico_City";
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    /// <summary>Convierte un instante UTC a la zona de la empresa.</summary>
    public static DateTimeOffset Local(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, Tz);

    /// <summary>Fecha y hora actuales en la zona de la empresa.</summary>
    public static DateTimeOffset Ahora() => Local(DateTimeOffset.UtcNow);

    /// <summary>Fecha de hoy en la zona de la empresa.</summary>
    public static DateOnly Hoy() => DateOnly.FromDateTime(Ahora().DateTime);
}
