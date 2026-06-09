using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Domain.Entities.Interfaces.Common
{
    public interface ICommonPlayer
    {
        int CurrentHand { get; }
        List<int> AceValues { get; }
        IReadOnlyDictionary<int, IReadOnlyDictionary<CardVisibility, IReadOnlyList<Card>>> HandOfCards { get; }
        NamePlayers TypePlayer { get; }
        void AddCardToHand(int hand, Card card, CardVisibility visibility);
        void DiscardCardFromHand(int hand, Card card);
        void ChangeHand(int hand);
        void UpdateVisibilityAllCards();
        bool AddValueAce(int value);
    }
}