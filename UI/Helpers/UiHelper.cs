using BlackJack.Application;
using BlackJack.Application.Enums;
using BlackJack.Application.Helpers;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;
using BlackJack.UI.Sections;
using Spectre.Console;

namespace BlackJack.UI.Helpers;

public static class UiHelper
{
    public static Card? ValidateDealTheCard()
    {
        var card = Game.DealTheCard(CardVisibility.Visible);
        if (card != null)
        {
            var currentCard = (Card)card;
            var valueCard = currentCard.GetValue();
            var valueCardStr = valueCard == 11 ? "?" : valueCard.ToString();
            Widget.Card(currentCard, valueCardStr);
            return currentCard;
        }

        AnsiConsole.MarkupLineInterpolated($"[bold red]✗ No hay más cartas para repartir.");
        return null;
    }

    public static void SetValueAceCards(List<Card> listCards)
    {
        for (var i = 0; i < listCards.Count; i++)
        {
            var card = listCards[i];
            var valueCard = card.GetValue();
            var valueCardStr = valueCard == 11 ? "?" : valueCard.ToString();
            if (card.Rank != CardRank.Ace) continue;
            do
            {
                int forcedAceValue;
                if (Game.CurrentPlayer == NamePlayers.Player)
                {
                    Widget.Card(card, valueCardStr, i);
                    forcedAceValue = AnsiConsole.Ask<int>($"Establece un valor del 1 al 11 a tu AS (#{i + 1}): ");
                }
                else
                {
                    var random = new Random();
                    forcedAceValue = random.Next(1, 12);
                }

                if (Game.DealTheCard(CardVisibility.Visible, card, forcedAceValue) == null) break;
            } while (true);
        }
    }

    public static void SelectGameOption()
    {
        var arrOptions = new Dictionary<string, int>()
        {
            { "Dividir", 0 },
            { "Pedir más cartas", 1 },
            { "Jugar las cartas", 2 },
            { "Doblar apuesta", 3 },
            { "Rendirse", 4 }
        };
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Elije una opción")
                .AddChoices(arrOptions.Keys));
        var canPlayerContinueChoose = true;
        while (canPlayerContinueChoose)
        {
            switch (arrOptions[option])
            {
                case 0:
                    Divide();
                    break;
                case 1:
                    ValidateDealTheCard();
                    break;
                case 2:
                    PlayGame();
                    canPlayerContinueChoose = false;
                    break;
                case 3:
                    DuplicateBet();
                    PlayGame();
                    canPlayerContinueChoose = false;
                    break;
                case 4:
                    Surrender();
                    canPlayerContinueChoose = false;
                    break;
                default:
                    AnsiConsole.MarkupLineInterpolated(
                        $"[bold red]✗[/] Error inesperado");
                    canPlayerContinueChoose = false;
                    break;
            }
        }
    }

    private static void PlayGame()
    {
        Game.PlayCards();
        AnsiConsole.MarkupLine("[green]Jugaste tus cartas[/]");
        AnsiConsole.MarkupLine("[yellow]El Crupier mostro sus cartas[/]");
        if (!Validations.ValidateIfCrupierHaEnoughScore())
        {
            AnsiConsole.MarkupLine("[yellow]El Crupier no tiene suficientes puntos[/]");
            do
            {
                AnsiConsole.MarkupLine("[yellow]El Crupier tomo una carta[/]");
                Game.CrupierDealTheCard(false, 1);
            } while (!Validations.ValidateIfCrupierHaEnoughScore());
        }

        var listStatus = Game.ValidateWinner();
        for (int i = 0; i < listStatus.Count; i++)
        {
            var extraInfo = listStatus.Count == 1 ? $"en tu mano {i}" : "";
            var status = listStatus[i];
            switch (status)
            {
                case WinnerStatus.Winner:
                    AnsiConsole.MarkupLine($"[green]Felicidades ganaste {extraInfo}[/]");
                    break;
                case WinnerStatus.WinnerWithBlackJack:
                    AnsiConsole.MarkupLine($"[green]Felicidades ganaste con un BlackJack Natural {extraInfo}[/]");
                    break;
                case WinnerStatus.Loser:
                    AnsiConsole.MarkupLineInterpolated($"[bold red]Perdiste {extraInfo}[/]");
                    break;
                case WinnerStatus.Draw:
                    AnsiConsole.MarkupLineInterpolated(
                        $"[yellow]Tu y el Curier obtuvieron el mismo puntaje {extraInfo}[/]");
                    break;
                default:
                    AnsiConsole.MarkupLineInterpolated(
                        $"[bold red]✗[/] Error inesperado");
                    break;
            }
        }

        Dialog.GameOver = true;
    }

    private static void Divide()
    {
        var statusAddHand = Game.AddHand();
        switch (statusAddHand)
        {
            case AddHandStatus.PlayerHasNotEnoughCoins:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Insuficientes fichas de casino para crear una nueva mano");
                break;
            case AddHandStatus.PlayerHasNotTwoCards:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] No tienes dos cartas del mismo valor para dividir");
                break;
            case AddHandStatus.Success:
                AnsiConsole.MarkupLine("[green]✓ Se creo una nueva mano[/]");
                break;
            default:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Error inesperado");
                break;
        }
    }

    private static void DuplicateBet()
    {
        var statusDuplicateBet = Game.DuplicateBet();
        switch (statusDuplicateBet)
        {
            case DuplicateBetStatus.PlayerHasNotEnoughCoins:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Insuficientes fichas de casino para duplicar la apuesta");
                break;
            case DuplicateBetStatus.Success:
                AnsiConsole.MarkupLine("[green]✓ Se duplico su apuesta[/]");
                break;
            default:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Error inesperado");
                break;
        }
    }

    public static bool AskIfWantToPlay()
    {
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("¿Quiere volver a jugar?")
                .AddChoices("Si", "No")
        );
        switch (option)
        {
            case "Si":
                return true;
            case "No":
                Environment.Exit(0);
                return false;
            default:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Error inesperado");
                return false;
        }
    }

    private static void Surrender()
    {
        Dialog.GameOver = true;
        Game.Surrender();
        AnsiConsole.MarkupLine("[green]✓ Se reembolsaron la mitad de tus fichas de casino[/]");
    }
}