using Brasa.Application.Shared;

namespace Brasa.UnitTests.Shared;

public class MoneyTests
{
    [Theory]
    [InlineData("24.90", 2490)]
    [InlineData("0.01", 1)]
    [InlineData("9999.99", 999999)]
    public void Converte_reais_em_centavos(string amount, long cents)
    {
        var value = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(cents, Money.ToCents(value));
        Assert.Equal(value, Money.FromCents(cents));
    }
}
