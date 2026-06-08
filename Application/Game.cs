using BlackJack.Application.Interfaces;
using BlackJack.Domain.Entities;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Helpers;
using BlackJack.Domain.Entities.Interfaces;
using BlackJack.Domain.Entities.Interfaces.Common;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Application
    {
    internal class Game : IGame
        {
        private readonly IPlayer _player;
        private readonly ICrupier _crupier;
        private readonly List<Card> _deck = [];
        public IPlayer Player => _player;
        public ICrupier Crupier => _crupier;
        public NamePlayers CurrentPlayer => NamePlayers.Player;
        public List<Card> Decks => _deck;

        public Game(Player player, Crupier crupier)
            {
            _player = player;
            _crupier = crupier;
            GenerateDeck();
            }

        private void GenerateDeck(int numberOfDecks = 6)
            {
            foreach (int _ in Enumerable.Range(0, numberOfDecks))
                {
                foreach (ShapeCard shape in Enum.GetValues<ShapeCard>())
                    {
                    foreach (CardRank rank in Enum.GetValues<CardRank>())
                        {
                        _deck.Add(new Card(shape, rank));
                        }
                    }
                }
            }

        public bool AddHand()
            {
            int originalBet = Player.CasinoChipsLog[FlowCash.Income][0];
            bool hasPlayerEnoughCoins = originalBet <= Player.CasinoChips;
            IEnumerable<Card> cards = Helper.GetAllCardsFromHand(Player.CurrentHand, Player.HandOfCards).ToList();

            if (!(hasPlayerEnoughCoins && cards.Any())) return false;

            bool hasPlayerTwoCardsWithTheSameValue =
                cards.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).Count() > 1;

            if (!(hasPlayerTwoCardsWithTheSameValue && BetCasinoChips(originalBet))) return false;

            int newHand = Player.HandOfCards.Last().Key + 1;
            Player.TranslateLastCardToNewHand(Player.CurrentHand, newHand);
            DealTheCard(CardVisibility.Hidden);
            Player.ChangeHand(newHand);
            DealTheCard(CardVisibility.Hidden);

            return true;
            }

        public bool BetCasinoChips(int casinoChips)
            {
            bool hasPlayerEnoughCoins = casinoChips <= Player.CasinoChips;
            if (!hasPlayerEnoughCoins) return false;
            Player.UpdateCasinoChips(-casinoChips, false);
            return true;
            }

        private Card? GetCardFromDeck()
            {
            Random random = new();
            int cardIndex = random.Next(0, Decks.Count);
            return Decks[cardIndex];
            }

        private void DiscardCardFromDeck(Card card)
            {
            Decks.Remove(card);
            }

        public Card? DealTheCard(CardVisibility visibility, int? forcedAceValue = null)
            {
            Card? card = GetCardFromDeck();
            if (card == null) return null;

            int aceValue = forcedAceValue ?? 11;

            ICommonPlayer? current = CurrentPlayer switch
                {
                NamePlayers.Player => Player,
                NamePlayers.Crupier => Crupier,
                _ => null
                };
            if (current == null) return null;
            Card actualCard = (Card)card;
            if (actualCard.Rank == CardRank.Ace)
                {
                current.AddValueAce(aceValue);
                }

            current.AddCardToHand(current.CurrentHand, actualCard, visibility);

            DiscardCardFromDeck(actualCard);
            return actualCard;
            }

        public bool DuplicateBet()
            {
            int duplicateBet = Player.CasinoChipsWagered * 2;
            bool hasPlayerEnoughCoins = duplicateBet <= Player.CasinoChips;
            if (!hasPlayerEnoughCoins && BetCasinoChips(duplicateBet)) return false;
            DealTheCard(CardVisibility.Hidden);
            Player.SetDoubleBet();
            return true;
            }

        public void PlayCards()
            {
            Player.UpdateVisibilityAllCards();
            Crupier.UpdateVisibilityAllCards();
            }

        public void RequestCasinoChips(int casinoChips)
            {
            Player.UpdateCasinoChips(+casinoChips, false);
            }

        public void Surrender()
            {
            Player.UpdateCasinoChips(+Player.CasinoChipsWagered / 2, true, true);
            }

        private static List<List<int>> GetValuesByHands(ICommonPlayer player)
            {
            List<List<int>> totalValues = [];
            int currentIndexHand = 0;
            foreach (int hand in player.HandOfCards.Keys.ToList())
                {
                totalValues.Add([]);
                int indexCardAce = 0;
                foreach (Card card in Helper.GetAllCardsFromHand(hand, player.HandOfCards))
                    {
                    if (card.Rank == CardRank.Ace)
                        {
                        int aceValue = player.AceValues[indexCardAce];
                        totalValues[currentIndexHand].Add(card.GetValue(aceValue));
                        indexCardAce++;
                        }
                    else
                        {
                        totalValues[currentIndexHand].Add(card.GetValue());
                        }
                    }

                currentIndexHand++;
                }

            return totalValues;
            }

        private void UpdateBet()
            {
            }

        public void ValidateWinner()
            {
            List<List<int>> valuesByHands = GetValuesByHands(Player);
            int crupierTotalValue = GetValuesByHands(Crupier)[0].Sum();
            int currentHand = 0;
            foreach (List<int> valuesByHand in valuesByHands)
                {
                int totalValue = valuesByHand.Sum();
                bool hasPlayerMoreScoreThanCrupier = totalValue > crupierTotalValue;
                bool hasCrupierMoreScoreThanPlayer = crupierTotalValue > totalValue;
                bool hasPlayerMoreScoreThan21 = totalValue > 21;
                bool hasCrupierMoreScoreThan21 = crupierTotalValue > 21;
                bool hasPlayerBlackjackNatural = valuesByHand[0] == 11 && valuesByHand[1] == 10;
                bool hasPlayerAndCrupierTheSameScore = totalValue == crupierTotalValue;
                int actualBetByHand = Player.CasinoChipsLog[FlowCash.Expenses][currentHand];
                if ((hasPlayerMoreScoreThanCrupier || hasCrupierMoreScoreThan21) && !hasPlayerMoreScoreThan21)
                    {
                    if (hasPlayerBlackjackNatural)
                        {
                        int moreCasinoChips = (actualBetByHand / 2) * 3;
                        Player.UpdateCasinoChips(+actualBetByHand + moreCasinoChips, true);
                        }
                    else
                        {
                        Player.UpdateCasinoChips(+actualBetByHand * 2, true);
                        }
                    }
                else if ((hasCrupierMoreScoreThanPlayer || hasPlayerMoreScoreThan21) && !hasCrupierMoreScoreThan21)
                    {
                    Player.UpdateCasinoChips(-actualBetByHand, true, true);
                    }
                else if (hasPlayerAndCrupierTheSameScore)
                    {
                    Player.UpdateCasinoChips(-actualBetByHand, true);
                    }

                currentHand++;
                }
            }
        }
    }