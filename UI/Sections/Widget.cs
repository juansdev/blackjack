using BlackJack.Application;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Helpers;
using BlackJack.Domain.Entities.Interfaces.Common;
using BlackJack.Domain.Entities.Record;
using Spectre.Console;

namespace BlackJack.UI.Sections;

public class Widget
{
    public static void Card(Card currentCard, string cardValue, int cardIndex = 0)
    {
        var panel = new Panel($"[bold]{currentCard.Rank} = {cardValue}[/]")
            .Header($"[yellow]{currentCard.Shape}[/]", Justify.Center)
            .DoubleBorder()
            .RoundedBorder()
            .BorderColor(Color.White)
            .Padding(2, 1);

        AnsiConsole.Write(panel);
    }

    public static void ShowPlayerInfoWithCards(ICommonPlayer player)
    {
        ShowPlayerInfo();
        ShowAllCards(player);
    }

    public static void ShowPlayerInfo()
    {
        AnsiConsole.MarkupLineInterpolated($"[yellow]Disponible: {Game.Player.CasinoChips} fichas[/]");
        AnsiConsole.MarkupLineInterpolated($"[yellow]En la apuesta: {Game.Player.CasinoChipsWagered} fichas[/]");
    }

    public static void ShowAllCards(ICommonPlayer player)
    {
        Console.WriteLine("");
        var namePlayer = NamePlayers.Player == player.TypePlayer ? "Jugador" : "Crupier";

        var mainGrid = new Grid { Expand = true };
        mainGrid.AddColumn();

        mainGrid.AddRow($"[bold yellow]Baraja del {namePlayer} [/]");
        mainGrid.AddEmptyRow();

        var deckGrid = new Grid();
        deckGrid.AddColumns(3);
        deckGrid.AddRow("# Mano", "Figura", "Rango");
        int indexCardAce = 0;
        foreach (var hand in player.HandOfCards.Keys.ToList())
        {
            foreach (var card in Helper.GetAllCardsFromHand(hand, player.HandOfCards))
            {
                var cardValue = card.GetValue();
                if (card.Rank == CardRank.As)
                {
                    cardValue = card.GetValue(player.AceValues[indexCardAce]);
                    indexCardAce++;
                }

                deckGrid.AddRow($"[green]{(hand + 1).ToString()}[/]",
                    $"[yellow]{card.Shape.ToString()}[/]",
                    $"[red]{card.Rank.ToString()} = {cardValue}[/]");
            }
        }

        mainGrid.AddRow(deckGrid);

        AnsiConsole.Write(mainGrid);
        Console.WriteLine("");
    }
}