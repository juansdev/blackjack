using BlackJack.Domain.Entities.Common;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Helpers;
using BlackJack.Domain.Entities.Interfaces;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Domain.Entities
{
    public class Player : CommonPlayer, IPlayer
    {
        private readonly Dictionary<FlowCash, List<int>> _casinoChipsLog = [];
        private int _casinoChips = 0;
        private int _casinoChipsWagered = 0;
        private bool _hasPlayerDoubledTheBed = false;
        private string _name = "";
        public string Name => _name;
        public Dictionary<FlowCash, List<int>> CasinoChipsLog => _casinoChipsLog;
        public int CasinoChips => _casinoChips;
        public int CasinoChipsWagered => _casinoChipsWagered;
        public bool HasPlayerDoubledTheBed => _hasPlayerDoubledTheBed;

        public void UpdateCasinoChips(int casinoChips, bool isRefund, bool isDefeat = false)
        {
            int normalizeChips = Math.Abs(casinoChips);
            if (isRefund)
            {
                _casinoChipsWagered -= normalizeChips;
                CasinoChipsLog.TryGetValue(FlowCash.Income, out var expensesValues);
                if (expensesValues == null)
                {
                    CasinoChipsLog[FlowCash.Income] = [];
                }

                CasinoChipsLog[FlowCash.Income].Add(normalizeChips);
                if (isDefeat) _casinoChips -= +(normalizeChips);
                else _casinoChips += normalizeChips;
            }
            else if (casinoChips < 0 && !isRefund)
            {
                _casinoChipsWagered += normalizeChips;
                CasinoChipsLog.TryGetValue(FlowCash.Expenses, out var incomeValues);
                if (incomeValues == null)
                {
                    CasinoChipsLog[FlowCash.Expenses] = [];
                }

                CasinoChipsLog[FlowCash.Expenses].Add(normalizeChips);
                _casinoChips -= normalizeChips;
            }
            else _casinoChips += normalizeChips;
        }

        public void UpdateName(string name) => _name = name;

        public void TranslateLastCardToNewHand(int fromHand, int destinationHand)
        {
            Card lastCard = Helper.GetAllCardsFromHand(fromHand, HandOfCards).Last();
            AddCardToHand(destinationHand, lastCard, CardVisibility.Hidden);
            DiscardCardFromHand(fromHand, lastCard);
        }

        public void SetDoubleBet() => _hasPlayerDoubledTheBed = true;
    }
}