using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRCPoker;

namespace Tests
{
    public class PokerHandEvaluationTest
    {
        [Test]
        public void HighCardWins()
        {
            CardHand common = new GameObject().AddComponent<CardHand>();
            common.cardRanks = new Rank[] { Rank.Seven, Rank.Six, Rank.Three, Rank.Queen, Rank.Two };
            common.cardSuits = new Suit[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Clubs, Suit.Diamonds };

            CardHand p0 = new GameObject().AddComponent<CardHand>();
            p0.cardRanks = new Rank[] { Rank.Five, Rank.Nine };
            p0.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            CardHand p1 = new GameObject().AddComponent<CardHand>();
            p1.cardRanks = new Rank[] { Rank.King, Rank.Eight };
            p1.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            CardHand p2 = new GameObject().AddComponent<CardHand>();
            p2.cardRanks = new Rank[] { Rank.Eight, Rank.Jack };
            p2.cardSuits = new Suit[] { Suit.Clubs, Suit.Hearts };

            int[] winningHands = WinningHandSolver.GetWinningHands(common, new CardHand[] { p0, p1, p2 });
            Assert.That(winningHands.Length == 1); // Exactly one winner
            Assert.That(winningHands[0] == 1); // Player 2 (index 1) wins
        }
    }

}
