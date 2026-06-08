using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Interfaces.Common;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Domain.Entities.Common
{
    public class CommonPlayer : ICommonPlayer
    {
        private int _currentHand;
        private readonly List<int> _aceValues = [];
        private readonly Dictionary<int, Dictionary<CardVisibility, List<Card>>> _handOfCards = [];

        public IReadOnlyDictionary<int, IReadOnlyDictionary<CardVisibility, IReadOnlyList<Card>>> HandOfCards
            => _handOfCards.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyDictionary<CardVisibility, IReadOnlyList<Card>>)kvp.Value.ToDictionary(
                    inner => inner.Key,
                    inner => (IReadOnlyList<Card>)inner.Value
                )
            );
        
        public int CurrentHand => _currentHand;
        public List<int> AceValues => _aceValues;
        
        public void AddCardToHand(int hand, Card card, CardVisibility visibility)
        {
            if (!_handOfCards.TryGetValue(hand, out var handDict))
            {
                _handOfCards[hand] = [];
            }

            if (handDict == null) return;
            if (!handDict.TryGetValue(visibility, out _))
            {
                handDict[visibility] = [];
            }
            else
            {
                handDict[visibility].Add(card);
            }
        }

        public void DiscardCardFromHand(int hand, Card card)
        {
            _handOfCards[hand].Select(item => item.Value).First(v => v.Contains(card)).Remove(card);
        }

        public void ChangeHand(int hand) => _currentHand = hand;

        public void UpdateVisibilityAllCards()
        {
            foreach (var hand in _handOfCards.Select(x=>x.Key))
            {
                var listVisibleCard = _handOfCards[hand][CardVisibility.Visible];
                var listHiddenCard = _handOfCards[hand][CardVisibility.Hidden];
                if (listHiddenCard.Count <= 0) return;
                _handOfCards[hand][CardVisibility.Visible] = [.. listVisibleCard, .. listHiddenCard];
                _handOfCards[hand][CardVisibility.Hidden] = [];
            }
        }

        public bool AddValueAce(int value)
        {
            if (!Enumerable.Range(1,11).Contains(value))
            {
                return false;
            }
            AceValues.Add(value);
            return true;
        }
    }
}