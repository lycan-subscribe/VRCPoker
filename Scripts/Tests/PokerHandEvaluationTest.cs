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
        private CardHand common, p0, p1, p2;

        [SetUp]
        public void CardHands()
        {
            common = new GameObject().AddComponent<CardHand>();
            common.cardRanks = new Rank[] { Rank.Seven, Rank.Six, Rank.Three, Rank.Queen, Rank.Two };
            common.cardSuits = new Suit[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Clubs, Suit.Diamonds };

            p0 = new GameObject().AddComponent<CardHand>();
            p0.cardRanks = new Rank[] { Rank.Five, Rank.Nine };
            p0.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            p1 = new GameObject().AddComponent<CardHand>();
            p1.cardRanks = new Rank[] { Rank.King, Rank.Eight };
            p1.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            p2 = new GameObject().AddComponent<CardHand>();
            p2.cardRanks = new Rank[] { Rank.Eight, Rank.Jack };
            p2.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };
        }

        [Test]
        public void HighCardWins()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { p0, p1, p2 });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 1);
        }

        [Test]
        public void OnlyOnePlayer()
        {
            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { p0 });
            Assert.That(winningHands.Length == 1);
            Assert.That(winningHands[0] == 0);
        }
    }

}
