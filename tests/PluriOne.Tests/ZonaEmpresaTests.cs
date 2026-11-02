using PluriOne.Shared;
using Xunit;

namespace PluriOne.Tests;

public class ZonaEmpresaTests
{
    [Fact]
    public void Local_convierte_UTC_a_zona_de_la_empresa()
    {
        // 03:00 UTC del día 2 = 21:00 del día 1 en America/Mexico_City (UTC-6)
        var utc = new DateTimeOffset(2026, 11, 2, 3, 0, 0, TimeSpan.Zero);

        var local = ZonaEmpresa.Local(utc);

        Assert.Equal(2026, local.Year);
        Assert.Equal(11, local.Month);
        Assert.Equal(1, local.Day);
    }
}
