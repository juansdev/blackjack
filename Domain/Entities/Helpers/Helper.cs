using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack.Domain.Entities.Helpers
    {
    public class Helper
        {
            public static IEnumerable<Card> GetAllCardsFromHand(int currentHand, IReadOnlyDictionary<int, IReadOnlyDictionary<CardVisibility, IReadOnlyList<Card>>> handOfCards)
            {
                IEnumerable<Card> cards = [];
                if (handOfCards.TryGetValue(currentHand, out var visDict)
                && visDict.TryGetValue(CardVisibility.Visible, out var visibleCards)
                && visDict.TryGetValue(CardVisibility.Hidden, out var hiddenCards))
                {
                    cards = visibleCards.Concat(hiddenCards);
                }
                return cards;
            }
        }
    }
