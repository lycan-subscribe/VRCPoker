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
        // A Test behaves as an ordinary method
        [Test]
        public void PokerHandEvaluationTestSimplePasses()
        {
            // Use the Assert class to test conditions
            Assert.True(1 == 2);
        }

        [Test]
        public void DisableCardHand()
        {
            CardHand hand = new GameObject().AddComponent<CardHand>();
        }
    }
}
