﻿using System.Collections;
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
        private CardHand common, highNine, highKing, highJack, pair, twoPair;

        [SetUp]
        public void CardHands()
        {
            common = new GameObject().AddComponent<CardHand>();
            common.cardRanks = new Rank[] { Rank.Seven, Rank.Six, Rank.Three, Rank.Queen, Rank.Two };
            common.cardSuits = new Suit[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Clubs, Suit.Diamonds };

            highNine = new GameObject().AddComponent<CardHand>();
            highNine.cardRanks = new Rank[] { Rank.Five, Rank.Nine };
            highNine.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            highKing = new GameObject().AddComponent<CardHand>();
            highKing.cardRanks = new Rank[] { Rank.King, Rank.Eight };
            highKing.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            highJack = new GameObject().AddComponent<CardHand>();
            highJack.cardRanks = new Rank[] { Rank.Eight, Rank.Jack };
            highJack.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            pair = new GameObject().AddComponent<CardHand>();
            pair.cardRanks = new Rank[] { Rank.Two, Rank.Jack };
            pair.cardSuits = new Suit[] { Suit.Spades, Suit.Diamonds };

            twoPair = new GameObject().AddComponent<CardHand>();
            twoPair.cardRanks = new Rank[] { Rank.Two, Rank.Three };
            twoPair.cardSuits = new Suit[] { Suit.Hearts, Suit.Spades };
        }

        [Test]
        public void HighCardWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { highNine, highKing, highJack });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 1);
        }

        [Test]
        public void OnePairWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { highNine, highKing, highJack, pair });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 3);
        }

        [Test]
        public void TwoPairWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { highNine, highKing, twoPair, highJack, pair });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 2);
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
    }

}
