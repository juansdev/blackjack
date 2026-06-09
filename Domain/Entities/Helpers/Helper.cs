using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Domain.Entities.Helpers
{
    public class Helper
    {
        public static IEnumerable<Card> GetAllCardsFromHand(int currentHand,
            IReadOnlyDictionary<int, IReadOnlyDictionary<CardVisibility, IReadOnlyList<Card>>> handOfCards)
        {
            handOfCards.TryGetValue(currentHand, out var visDict);
            if (visDict == null) return [];
            visDict.TryGetValue(CardVisibility.Visible, out var visibleCards);
            visDict.TryGetValue(CardVisibility.Hidden, out var hiddenCards);
            IEnumerable<Card> cards = (visibleCards ?? []).Concat(hiddenCards ?? []);
            return cards;
        }
    }
}