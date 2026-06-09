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
    public static void FullValidateDealTheCard(CardVisibility visibility)
    {
        var card = ValidateDealTheCard(visibility);
        if (card != null)
        {
            var currentCard = (Card)card;
            SetValueAceCards([currentCard]);
        }
    }

    public static Card? ValidateDealTheCard(CardVisibility visibility)
    {
        var card = Game.DealTheCard(visibility);
        if (card != null)
        {
            var actualCard = (Card)card;
            if (actualCard.Rank == CardRank.As)
            {
                AnsiConsole.MarkupLine("[green]Te salio un AS[/]");
            }

            return (Card)card;
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
            if (card.Rank != CardRank.As) continue;
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

                if (Game.DealTheCard(CardVisibility.Visible, card, forcedAceValue) != null) break;
            } while (true);
        }
    }

    public static void SelectGameOption()
    {
        AnsiConsole.Clear();
        Head.MainHead();
        Widget.ShowPlayerInfoWithCards(Game.Player);
        var arrOptions = new Dictionary<string, int>()
        {
            { "Dividir", 0 },
            { "Pedir más cartas", 1 },
            { "Jugar las cartas", 2 },
            { "Doblar apuesta", 3 },
            { "Rendirse", 4 }
        };
        var canPlayerContinueChoose = true;
        while (canPlayerContinueChoose)
        {
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Elije una opción")
                    .AddChoices(arrOptions.Keys));
            switch (arrOptions[option])
            {
                case 0:
                    if (Divide())
                    {
                        AnsiConsole.Clear();
                        Head.MainHead();
                        Widget.ShowPlayerInfoWithCards(Game.Player);
                    }

                    break;
                case 1:
                    FullValidateDealTheCard(CardVisibility.Hidden);
                    AnsiConsole.Clear();
                    Head.MainHead();
                    Widget.ShowPlayerInfoWithCards(Game.Player);
                    break;
                case 2:
                    PlayGame();
                    canPlayerContinueChoose = false;
                    break;
                case 3:
                    if (DuplicateBet())
                    {
                        PlayGame();
                        canPlayerContinueChoose = false;
                    }

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

        AnsiConsole.Clear();
        Head.MainHead();
        Widget.ShowPlayerInfo();
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

        Widget.ShowAllCards(Game.Player);
        Widget.ShowAllCards(Game.Crupier);

        var listStatus = Game.ValidateWinner();
        var messageCrupierInfo = $"[yellow]Total puntos del Crupier: {Game.CrupierTotalValue}[/]";
        for (var i = 0; i < listStatus.Count; i++)
        {
            var extraInfo = listStatus.Count > 1 ? $"en tu mano {(i + 1).ToString()}" : "";
            var messagePlayerInfo =
                $"[yellow]Total puntos del Jugador {extraInfo}: {Game.EndValuesByHands[i].Sum()}[/]";
            var status = listStatus[i];
            switch (status)
            {
                case WinnerStatus.Winner:
                    AnsiConsole.MarkupLine(messageCrupierInfo);
                    AnsiConsole.MarkupLine(messagePlayerInfo);
                    AnsiConsole.MarkupLine($"[green]Felicidades ganaste {extraInfo}[/]");
                    break;
                case WinnerStatus.WinnerWithBlackJack:
                    AnsiConsole.MarkupLine(messageCrupierInfo);
                    AnsiConsole.MarkupLine(messagePlayerInfo);
                    AnsiConsole.MarkupLine($"[green]Felicidades ganaste con un BlackJack Natural {extraInfo}[/]");
                    break;
                case WinnerStatus.Loser:
                    AnsiConsole.MarkupLine(messageCrupierInfo);
                    AnsiConsole.MarkupLine(messagePlayerInfo);
                    AnsiConsole.MarkupLineInterpolated($"[bold red]Perdiste {extraInfo}[/]");
                    break;
                case WinnerStatus.Draw:
                    AnsiConsole.MarkupLine(messageCrupierInfo);
                    AnsiConsole.MarkupLine(messagePlayerInfo);
                    AnsiConsole.MarkupLineInterpolated(
                        $"[yellow]Tu y el Crupier obtuvieron el mismo puntaje {extraInfo}[/]");
                    break;
                default:
                    AnsiConsole.MarkupLineInterpolated(
                        $"[bold red]✗[/] Error inesperado");
                    break;
            }
        }
    }

    private static bool Divide()
    {
        var statusAddHand = Game.AddHand();
        switch (statusAddHand)
        {
            case AddHandStatus.PlayerHasNotEnoughCoins:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Insuficientes fichas de casino para crear una nueva mano");
                AnsiConsole.MarkupLineInterpolated(
                    $"[#FFA500]⚠[/] [yellow]Debes tener la misma cantidad de fichas de tu apuesta original[/]");
                break;
            case AddHandStatus.PlayerHasNotTwoCards:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] No tienes dos cartas del mismo valor para dividir");
                break;
            case AddHandStatus.Success:
                AnsiConsole.MarkupLine("[green]✓ Se creo una nueva mano[/]");
                return true;
            default:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Error inesperado");
                break;
        }

        return false;
    }

    private static bool DuplicateBet()
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
                return true;
            default:
                AnsiConsole.MarkupLineInterpolated(
                    $"[bold red]✗[/] Error inesperado");
                break;
        }

        return false;
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
        Game.Surrender();
        AnsiConsole.MarkupLine("[green]✓ Se reembolsaron la mitad de tus fichas de casino[/]");
    }
}