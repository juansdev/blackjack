using BlackJack.Application;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;
using BlackJack.UI.Helpers;
using BlackJack.UI.Sections;
using Spectre.Console;

namespace BlackJack.UI;

public class Dialog
{
    public static void DeployGame()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Head.MainHead();

            Game.StartGame();
            var casinoChips = AnsiConsole.Ask<int>("¿Cuantas fichas del casino desea?");
            Game.RequestCasinoChips(casinoChips);

            var betCasinoChips = AnsiConsole.Ask<int>("¿Cuantas fichas quieres apostar en el BlackJack?");
            Game.BetCasinoChips(betCasinoChips);

            AnsiConsole.Clear();
            Head.MainHead();
            Widget.ShowPlayerInfo();

            List<Card> listCards = [];
            AnsiConsole.Status()
                .Start("Repartiendo cartas...", ctx =>
                {
                    for (var i = 0; i < new int[2].Length; i++)
                    {
                        ctx.Status($"Repartiendo cartas... ({i + 1}/2)");
                        var card = UiHelper.ValidateDealTheCard(CardVisibility.Visible);
                        if (card == null) return;
                        var actualCard = (Card)card;

                        listCards.Add(actualCard);
                    }
                });
            UiHelper.SetValueAceCards(listCards);

            Game.CrupierDealTheCard();
            UiHelper.SelectGameOption();
            if (!UiHelper.AskIfWantToPlay())
            {
                Environment.Exit(0);
                break;
            }

            Game.RestartGame();
        }
    }
}