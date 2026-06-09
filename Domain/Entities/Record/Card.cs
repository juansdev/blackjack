using BlackJack.Domain.Entities.Enums;

namespace BlackJack.Domain.Entities.Record
{
    public readonly record struct Card(ShapeCard Shape, CardRank Rank)
    {
        private int BaseValue => Rank switch
        {
            CardRank.Jack or CardRank.Reina or CardRank.Rey => 10,
            CardRank.As => 11,
            _ => (int)Rank + 1
        };

        public int GetValue(int aceValue = 11) => Rank == CardRank.As ? aceValue : BaseValue;
    }
}