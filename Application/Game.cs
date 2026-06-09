using BlackJack.Application.Enums;
using BlackJack.Domain.Entities;
using BlackJack.Domain.Entities.Enums;
using BlackJack.Domain.Entities.Helpers;
using BlackJack.Domain.Entities.Interfaces;
using BlackJack.Domain.Entities.Interfaces.Common;
using BlackJack.Domain.Entities.Record;
using BlackJack.UI.Helpers;

namespace BlackJack.Application
    {
    internal class Game
        {
        private static Game? _instance;

        private Game()
            {
            CurrentPlayer = NamePlayers.Player;
            Player = new Player();
            Crupier = new Crupier();
            Decks = [];
            EndValuesByHands = [];
            CrupierTotalValue = 0;
            GenerateDeck();
            }

        public static IPlayer Player { get; private set; }
        public static ICrupier Crupier { get; private set; }
        public static NamePlayers CurrentPlayer { get; private set; }

        public static List<Card> Decks { get; private set; }

        public static List<List<int>> EndValuesByHands { get; private set; }
        public static int CrupierTotalValue { get; private set; }

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
                        Decks.Add(new Card(shape, rank));
                        }
                    }
                }
            }

        public static AddHandStatus AddHand()
            {
            Player.CasinoChipsLog.TryGetValue(FlowCash.Expenses, out var listOriginalBet);
            var originalBet = listOriginalBet != null ? listOriginalBet[0] : 0;
            bool hasPlayerEnoughCoins = originalBet <= Player.CasinoChips;
            IEnumerable<Card> cards = Helper.GetAllCardsFromHand(Player.CurrentHand, Player.HandOfCards).ToList();

            if (!(hasPlayerEnoughCoins && cards.Any())) return AddHandStatus.PlayerHasNotEnoughCoins;

            bool hasPlayerTwoCardsWithTheSameValue =
                cards.GroupBy(x => x.GetValue()).Count(g => g.Count() > 1) >= 1;

            if (!(hasPlayerTwoCardsWithTheSameValue && BetCasinoChips(originalBet)))
                return AddHandStatus.PlayerHasNotTwoCards;

            int newHand = Player.CurrentHand + 1;
            Player.TranslateLastCardToNewHand(Player.CurrentHand, newHand);
            UiHelper.FullValidateDealTheCard(CardVisibility.Hidden);
            Player.ChangeHand(newHand);
            UiHelper.FullValidateDealTheCard(CardVisibility.Hidden);

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

            ICommonPlayer current = CurrentPlayer == NamePlayers.Player ? Player : Crupier;
            Card actualCard = (Card)card;
            if (actualCard.Rank == CardRank.As)
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
            UiHelper.FullValidateDealTheCard(CardVisibility.Hidden);
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
            List<List<int>> totalValues = []; // [[1,2],[3,4]]
            int currentIndexHand = 0;
            int indexCardAce = 0;
            foreach (int hand in player.HandOfCards.Keys.ToList())
                {
                totalValues.Add([]);
                foreach (Card card in Helper.GetAllCardsFromHand(hand, player.HandOfCards))
                    {
                    if (card.Rank == CardRank.As)
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
            EndValuesByHands = GetValuesByHands(Player);
            CrupierTotalValue = GetValuesByHands(Crupier)[0].Sum();
            List<WinnerStatus> listStatus = [];
            int currentHand = 0;
            foreach (List<int> valuesByHand in EndValuesByHands)
                {
                int totalValue = valuesByHand.Sum();
                bool hasPlayerMoreScoreThanCrupier = totalValue > CrupierTotalValue;
                bool hasCrupierMoreScoreThanPlayer = CrupierTotalValue > totalValue;
                bool hasPlayerMoreScoreThan21 = totalValue > 21;
                bool hasCrupierMoreScoreThan21 = CrupierTotalValue > 21;
                bool hasPlayerBlackjackNatural = valuesByHand[0] == 11 && valuesByHand[1] == 10;
                bool hasPlayerAndCrupierTheSameScore = totalValue == CrupierTotalValue;
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
                if (card.Rank != CardRank.As) continue;
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