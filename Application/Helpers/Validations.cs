using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Helpers;

namespace BlackJack.Application.Helpers
    {
    public static class Validations
        {
        public static bool ValidateIfCrupierHaEnoughScore()
            {
            var totalScore = 0;
            var cards = Helper.GetAllCardsFromHand(0, Game.Crupier.HandOfCards).ToArray();
            for (int i = 0; i < cards.Length; i++)
                {
                var card = cards[i];
                var value = card.GetValue();
                if (card.Rank == CardRank.As)
                    {
                    var aceValue = Game.Crupier.AceValues[i];
                    value = card.GetValue(aceValue);
                    }

                totalScore += value;
                }

            return totalScore >= 16;
            }
        }
    }