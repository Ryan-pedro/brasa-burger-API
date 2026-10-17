namespace Brasa.Domain.Printing;

public sealed record PrintDocument(int LayoutVersion, IReadOnlyList<PrintLine> Lines)
{
    public const int CurrentLayoutVersion = 1;

    public static PrintDocument Of(IReadOnlyList<PrintLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        if (lines.Count == 0)
        {
            throw new ArgumentException("O documento precisa de linhas.", nameof(lines));
        }

        return new PrintDocument(CurrentLayoutVersion, lines);
    }
}
