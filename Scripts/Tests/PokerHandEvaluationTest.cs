using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRCPoker;

namespace Tests
{
    [TestFixture]
    public class PokerHandEvaluationTest
    {
        private CardHand common, common2, common3,
                         highNine, highKing, highJack, pair, twoPair,
                         threeOfKind, highKing2, flushEight, flushTen,
                         fullHouse, fourOfKind, straightFlush, royalFlush;

        [SetUp]
        public void CardHands()
        {
            common = new GameObject().AddComponent<CardHand>();
            common.cardRanks = new Rank[] { Rank.Seven, Rank.Six, Rank.Three, Rank.Queen, Rank.Two };
            common.cardSuits = new Suit[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Clubs, Suit.Diamonds };

            common2 = new GameObject().AddComponent<CardHand>();
            common2.cardRanks = new Rank[] { Rank.Seven, Rank.Six, Rank.Three, Rank.Four, Rank.Two };
            common2.cardSuits = new Suit[] { Suit.Spades, Suit.Clubs, Suit.Clubs, Suit.Clubs, Suit.Diamonds };

            common3 = new GameObject().AddComponent<CardHand>();
            common3.cardRanks = new Rank[] { Rank.Three, Rank.Queen, Rank.Queen, Rank.Jack, Rank.King };
            common3.cardSuits = new Suit[] { Suit.Hearts, Suit.Hearts, Suit.Spades, Suit.Spades, Suit.Spades };

            highNine = new GameObject().AddComponent<CardHand>();
            highNine.cardRanks = new Rank[] { Rank.Five, Rank.Nine };
            highNine.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            highKing = new GameObject().AddComponent<CardHand>();
            highKing.cardRanks = new Rank[] { Rank.King, Rank.Eight };
            highKing.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            highKing2 = new GameObject().AddComponent<CardHand>();
            highKing2.cardRanks = new Rank[] { Rank.King, Rank.Eight };
            highKing2.cardSuits = new Suit[] { Suit.Spades, Suit.Diamonds };

            highJack = new GameObject().AddComponent<CardHand>();
            highJack.cardRanks = new Rank[] { Rank.Eight, Rank.Jack };
            highJack.cardSuits = new Suit[] { Suit.Spades, Suit.Hearts };

            pair = new GameObject().AddComponent<CardHand>();
            pair.cardRanks = new Rank[] { Rank.Two, Rank.Jack };
            pair.cardSuits = new Suit[] { Suit.Spades, Suit.Diamonds };

            twoPair = new GameObject().AddComponent<CardHand>();
            twoPair.cardRanks = new Rank[] { Rank.Two, Rank.Three };
            twoPair.cardSuits = new Suit[] { Suit.Hearts, Suit.Spades };

            threeOfKind = new GameObject().AddComponent<CardHand>();
            threeOfKind.cardRanks = new Rank[] { Rank.Seven, Rank.Seven };
            threeOfKind.cardSuits = new Suit[] { Suit.Hearts, Suit.Diamonds };

            flushEight = new GameObject().AddComponent<CardHand>();
            flushEight.cardRanks = new Rank[] { Rank.Seven, Rank.Eight };
            flushEight.cardSuits = new Suit[] { Suit.Clubs, Suit.Clubs };

            flushTen = new GameObject().AddComponent<CardHand>();
            flushTen.cardRanks = new Rank[] { Rank.Two, Rank.Ten };
            flushTen.cardSuits = new Suit[] { Suit.Clubs, Suit.Clubs };

            fullHouse = new GameObject().AddComponent<CardHand>();
            fullHouse.cardRanks = new Rank[] { Rank.Queen, Rank.Three };
            fullHouse.cardSuits = new Suit[] { Suit.Diamonds, Suit.Clubs };

            fourOfKind = new GameObject().AddComponent<CardHand>();
            fourOfKind.cardRanks = new Rank[] { Rank.Queen, Rank.Queen };
            fourOfKind.cardSuits = new Suit[] { Suit.Diamonds, Suit.Clubs };

            straightFlush = new GameObject().AddComponent<CardHand>();
            straightFlush.cardRanks = new Rank[] { Rank.Nine, Rank.Ten };
            straightFlush.cardSuits = new Suit[] { Suit.Spades, Suit.Spades };

            royalFlush = new GameObject().AddComponent<CardHand>();
            royalFlush.cardRanks = new Rank[] { Rank.Ace, Rank.Ten };
            royalFlush.cardSuits = new Suit[] { Suit.Spades, Suit.Spades };
        }

        // Using Common1

        [Test]
        public void HighCardWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[]
                { highNine, highKing, highJack });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 1);
        }

        [Test]
        public void OnePairWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[]
                { highNine, highKing, highJack, pair });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 3);
        }

        [Test]
        public void TwoPairWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[]
                { highNine, highKing, twoPair, highJack, pair });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 2);
        }

        [Test]
        public void ThreeOfKindWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[]
                { highNine, threeOfKind, highKing, twoPair, highJack, pair });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 1);
        }

        [Test]
        public void OnlyOnePlayer()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { highNine });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 0);
        }

        [TearDown]
        public void handsNotMutated()
        {
            Assert.AreEqual(common.cardRanks, new Rank[] { Rank.Seven, Rank.Six, Rank.Three, Rank.Queen, Rank.Two });
            Assert.AreEqual(common.cardSuits, new Suit[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Clubs, Suit.Diamonds });

            Assert.AreEqual(highNine.cardRanks, new Rank[] { Rank.Five, Rank.Nine });
            Assert.AreEqual(highNine.cardSuits, new Suit[] { Suit.Clubs, Suit.Hearts });
        }

        // Using Commmon2

        [Test]
        public void StraightWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common2, new CardHand[]
                { twoPair, highNine });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 1);
        }

        [Test]
        public void FlushWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common2, new CardHand[]
                { twoPair, highNine, flushEight });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 2);
        }

        [Test]
        public void MultiFlush()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common2, new CardHand[]
                { flushTen, twoPair, highNine, flushEight });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 0);
        }

        // Uses Common3

        [Test]
        public void FullHouseWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common3, new CardHand[]
                { highNine, flushEight, fullHouse });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 2);
        }

        [Test]
        public void FourOfKindWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common3, new CardHand[]
                { highNine, flushEight, fourOfKind });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 2);
        }

        [Test]
        public void StraightFlushWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common3, new CardHand[]
                { straightFlush, highNine, flushEight, fourOfKind });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 0);
        }

        [Test]
        public void RoyalFlushWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common3, new CardHand[]
                { highNine, royalFlush, flushEight, fourOfKind });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 1);
        }

        [Test]
        public void highCardSplitPot()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[]
                { highKing, highNine, highKing2, highJack});
            Assert.That(winningHands.Length == 2);
            Assert.Contains(0, winningHands);
            Assert.Contains(2, winningHands);
        }
    }

}
