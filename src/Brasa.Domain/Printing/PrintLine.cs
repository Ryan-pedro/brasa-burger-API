namespace Brasa.Domain.Printing;

public sealed record PrintLine(PrintLineKind Kind, string Text, PrintAlign Align, bool Bold, bool Large)
{
    public static PrintLine Plain(string text) => new(PrintLineKind.Text, text, PrintAlign.Left, false, false);

    public static PrintLine Strong(string text) => new(PrintLineKind.Text, text, PrintAlign.Left, true, false);

    public static PrintLine Title(string text) => new(PrintLineKind.Text, text, PrintAlign.Center, true, true);

    public static PrintLine Centered(string text) => new(PrintLineKind.Text, text, PrintAlign.Center, false, false);

    public static readonly PrintLine Separator = new(PrintLineKind.Separator, string.Empty, PrintAlign.Left, false, false);

    public static readonly PrintLine Feed = new(PrintLineKind.Feed, string.Empty, PrintAlign.Left, false, false);

    public static readonly PrintLine Cut = new(PrintLineKind.Cut, string.Empty, PrintAlign.Left, false, false);
}
