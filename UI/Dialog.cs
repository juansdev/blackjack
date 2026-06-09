using BlackJack.Application;
using BlackJack.Domain.Entities;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;
using BlackJack.UI.Helpers;
using BlackJack.UI.Sections;
using Spectre.Console;

namespace BlackJack.UI;

public class Dialog
{
    private static readonly Player PrivPlayer = new Player();
    private static readonly Crupier PrivCrupier = new Crupier();
    public static Player Player => PrivPlayer;
    public static Crupier Crupier => PrivCrupier;
    public static bool GameOver { get; set; } = false;

    public static void DeployGame()
    {
        while (true)
        {
            Head.MainHead();

            Game.StartGame();
            var casinoChips = AnsiConsole.Ask<int>("¿Cuantas fichas del casino desea?");
            Game.RequestCasinoChips(casinoChips);

            var betCasinoChips = AnsiConsole.Ask<int>("¿Cuantas fichas quieres apostar en el BlackJack?");
            Game.BetCasinoChips(betCasinoChips);

            List<Card> listCards = [];
            AnsiConsole.Status()
                .Start("Repartiendo cartas... (0/2)", ctx =>
                {
                    Thread.Sleep(1500);
                    for (var i = 0; i < new int[2].Length; i++)
                    {
                        var card = UiHelper.ValidateDealTheCard();
                        if (card == null) return;
                        var actualCard = (Card)card;
                        if (actualCard.Rank == CardRank.Ace)
                        {
                            AnsiConsole.MarkupLine("[green]Te salio un AS[/]");
                        }

                        listCards.Add(actualCard);
                        ctx.Status($"Repartiendo cartas... ({i + 1}/2)");
                        Thread.Sleep(2000);
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
        }
    }
}