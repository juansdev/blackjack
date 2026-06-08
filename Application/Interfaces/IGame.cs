using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Interfaces;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Application.Interfaces
{
    internal interface IGame
    {
        IPlayer Player { get; }
        ICrupier Crupier { get; }
        NamePlayers CurrentPlayer { get; }
        List<Card> Decks { get; }
        void RequestCasinoChips(int casinoChips);
        bool BetCasinoChips(int casinoChips);
        bool AddHand();
        Card? DealTheCard(CardVisibility visibility, int? forcedAceValue = null);
        void PlayCards();
        bool DuplicateBet();
        void Surrender();
        void ValidateWinner();
    }
}
