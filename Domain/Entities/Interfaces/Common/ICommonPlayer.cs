using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack.Domain.Entities.Interfaces.Common
{
    internal interface ICommonPlayer
    {
        int CurrentHand { get; }
        List<int> AceValues { get; }
        IReadOnlyDictionary<int, IReadOnlyDictionary<CardVisibility, IReadOnlyList<Card>>> HandOfCards { get; }
        void AddCardToHand(int hand, Card card, CardVisibility visibility);
        void DiscardCardFromHand(int hand, Card card);
        void ChangeHand(int hand);
        void UpdateVisibilityAllCards();
        bool AddValueAce(int value);
    }
}
