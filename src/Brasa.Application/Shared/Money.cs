namespace Brasa.Application.Shared;

/// <summary>
/// No domínio e no banco, dinheiro é decimal com duas casas. Na API, trafega como inteiro
/// em centavos (ADR-0012): exato em qualquer linguagem, inclusive no navegador.
/// </summary>
public static class Money
{
    public static long ToCents(decimal amount) =>
        (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

    public static decimal FromCents(long cents) => cents / 100m;
}
