using Spectre.Console;

namespace BlackJack.UI.Sections;

public class Head
{
    public static void MainHead()
    {
        AnsiConsole.MarkupLine(
            "[bold yellow]¡Bienvenido al [green]C[/] [blue]A[/] [red]S[/] [violet]I[/] [white]N[/] [Cyan]O[/] ![/]");
    }
}