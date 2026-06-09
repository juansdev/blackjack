namespace BlackJack.Application.Enums
    {
    public enum AddHandStatus
        {
        Success = 0,
        PlayerHasNotEnoughCoins = 1,
        PlayerHasNotTwoCards = 2
        }

    public enum DuplicateBetStatus
        {
        Success = 0,
        PlayerHasNotEnoughCoins = 1,
        }

    public enum WinnerStatus
        {
        Winner = 0,
        WinnerWithBlackJack = 1,
        Loser = 2,
        Draw = 3
        }
    }