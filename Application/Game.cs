using BlackJack.Application.Enums;
using BlackJack.Domain.Entities;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Helpers;
using BlackJack.Domain.Entities.Interfaces;
using BlackJack.Domain.Entities.Interfaces.Common;
using BlackJack.Domain.Entities.Record;

namespace BlackJack.Application
    {
    internal class Game
        {
        private static Game? _instance;
        private static readonly IPlayer _player = new Player();
        private static readonly ICrupier _crupier = new Crupier();
        private static readonly List<Card> _deck = [];

        private Game()
            {
            CurrentPlayer = NamePlayers.Player;
            GenerateDeck();
            }

        public static IPlayer Player => _player;
        public static ICrupier Crupier => _crupier;
        public static NamePlayers CurrentPlayer { get; set; }

        public static List<Card> Decks => _deck;

        public static void StartGame()
            {
            _instance ??= new Game();
            }

        public static void RestartGame()
            {
            _instance = new Game();
            }

        private static void GenerateDeck(int numberOfDecks = 6)
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

        public static AddHandStatus AddHand()
            {
            int originalBet = Player.CasinoChipsLog[FlowCash.Income][0];
            bool hasPlayerEnoughCoins = originalBet <= Player.CasinoChips;
            IEnumerable<Card> cards = Helper.GetAllCardsFromHand(Player.CurrentHand, Player.HandOfCards).ToList();

            if (!(hasPlayerEnoughCoins && cards.Any())) return AddHandStatus.PlayerHasNotEnoughCoins;

            bool hasPlayerTwoCardsWithTheSameValue =
                cards.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).Count() > 1;

            if (!(hasPlayerTwoCardsWithTheSameValue && BetCasinoChips(originalBet)))
                return AddHandStatus.PlayerHasNotTwoCards;

            int newHand = Player.HandOfCards.Last().Key + 1;
            Player.TranslateLastCardToNewHand(Player.CurrentHand, newHand);
            DealTheCard(CardVisibility.Hidden);
            Player.ChangeHand(newHand);
            DealTheCard(CardVisibility.Hidden);

            return AddHandStatus.Success;
            }

        public static bool BetCasinoChips(int casinoChips)
            {
            bool hasPlayerEnoughCoins = casinoChips <= Player.CasinoChips;
            if (!hasPlayerEnoughCoins) return false;
            Player.UpdateCasinoChips(-casinoChips, false);
            return true;
            }

        private static Card? GetCardFromDeck()
            {
            Random random = new();
            int cardIndex = random.Next(0, Decks.Count);
            return Decks[cardIndex];
            }

        private static void DiscardCardFromDeck(Card card)
            {
            Decks.Remove(card);
            }

        public static Card? DealTheCard(CardVisibility visibility, Card? aceCard = null, int? forcedAceValue = null)
            {
            Card? card = aceCard ?? GetCardFromDeck();
            if (card == null) return null;

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
                if (forcedAceValue == null) return actualCard;
                if (!current.AddValueAce((int)forcedAceValue)) return null;
                }

            current.AddCardToHand(current.CurrentHand, actualCard, visibility);

            DiscardCardFromDeck(actualCard);
            return actualCard;
            }

        public static DuplicateBetStatus DuplicateBet()
            {
            int duplicateBet = Player.CasinoChipsWagered * 2;
            bool hasPlayerEnoughCoins = duplicateBet <= Player.CasinoChips;
            if (!hasPlayerEnoughCoins && BetCasinoChips(duplicateBet))
                return DuplicateBetStatus.PlayerHasNotEnoughCoins;
            DealTheCard(CardVisibility.Hidden);
            Player.SetDoubleBet();
            return DuplicateBetStatus.Success;
            }

        public static void PlayCards()
            {
            Player.UpdateVisibilityAllCards();
            Crupier.UpdateVisibilityAllCards();
            }

        public static void RequestCasinoChips(int casinoChips)
            {
            Player.UpdateCasinoChips(+casinoChips, false);
            }

        public static void Surrender()
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

        public static List<WinnerStatus> ValidateWinner()
            {
            List<List<int>> valuesByHands = GetValuesByHands(Player);
            List<WinnerStatus> listStatus = [];
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
                        listStatus.Add(WinnerStatus.WinnerWithBlackJack);
                        }
                    else
                        {
                        Player.UpdateCasinoChips(+actualBetByHand * 2, true);
                        listStatus.Add(WinnerStatus.Winner);
                        }
                    }
                else if ((hasCrupierMoreScoreThanPlayer || hasPlayerMoreScoreThan21) && !hasCrupierMoreScoreThan21)
                    {
                    Player.UpdateCasinoChips(-actualBetByHand, true, true);
                    listStatus.Add(WinnerStatus.Loser);
                    }
                else if (hasPlayerAndCrupierTheSameScore)
                    {
                    Player.UpdateCasinoChips(-actualBetByHand, true);
                    listStatus.Add(WinnerStatus.Draw);
                    }

                currentHand++;
                }

            return listStatus;
            }

        public static void SetValueAceCards(List<Card> listCards)
            {
            for (var i = 0; i < listCards.Count; i++)
                {
                var card = listCards[i];
                if (card.Rank != CardRank.Ace) continue;
                do
                    {
                    int forcedAceValue;
                    var random = new Random();
                    forcedAceValue = random.Next(1, 12);
                    if (DealTheCard(CardVisibility.Visible, card, forcedAceValue) == null) break;
                    } while (true);
                }
            }

        public static void CrupierDealTheCard(bool isInitial = true, int take = 2)
            {
            List<Card> listCards = [];
            CurrentPlayer = NamePlayers.Crupier;
            for (var i = 0; i < new int[take].Length; i++)
                {
                var card = DealTheCard(i == 0 || !isInitial ? CardVisibility.Visible : CardVisibility.Hidden);
                if (card == null) return;
                listCards.Add((Card)card);
                }

            SetValueAceCards(listCards);
            CurrentPlayer = NamePlayers.Player;
            }
        }
    }