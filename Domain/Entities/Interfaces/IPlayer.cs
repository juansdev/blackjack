using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack.Domain.Entities.Interfaces
{
    internal interface IPlayer : ICommonPlayer
    {
        string Name { get; }
        Dictionary<FlowCash, List<int>> CasinoChipsLog { get; }
        int CasinoChips { get; }
        int CasinoChipsWagered { get; }
        bool HasPlayerDoubledTheBed { get; }
        void UpdateName(string name);
        void UpdateCasinoChips(int casinoChips, bool isRefund, bool isDefeat = false);
        void TranslateLastCardToNewHand(int fromHand, int destinationHand);
        void SetDoubleBet();
    }
}
