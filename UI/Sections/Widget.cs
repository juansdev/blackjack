using BlackJack.Domain.Entities.Record;
using Spectre.Console;

namespace BlackJack.UI.Sections;

public class Widget
{
    public static void Card(Card currentCard, string cardValue, int cardIndex = 0)
    {
        var strCardIndex = cardIndex != 0 ? $"#{cardIndex}" : "";
        var panel = new Panel($"[bold]{currentCard.Rank} = {cardValue}{strCardIndex}[/]")
            .Header($"[yellow]{currentCard.Shape}[/]", Justify.Center)
            .DoubleBorder()
            .RoundedBorder()
            .BorderColor(Color.White)
            .Padding(2, 1);

        AnsiConsole.Write(panel);
    }
}