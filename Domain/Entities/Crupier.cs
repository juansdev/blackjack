using BlackJack.Domain.Entities.Common;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Interfaces;

namespace BlackJack.Domain.Entities
{
    public class Crupier : CommonPlayer, ICrupier
    {
        protected new readonly NamePlayers _typePlayer = NamePlayers.Crupier;
        public new NamePlayers TypePlayer => _typePlayer;
    }
}