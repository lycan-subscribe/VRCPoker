using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using System;

namespace VRCPoker{
    public abstract class GameMat : UdonSharpBehaviour
    {
        public VRCPlayerApi player = null;
        public CardHand hand;

        public abstract void MyTurn();
		public abstract void SomeoneElsesTurn();
		public abstract void WaitingForTurn();
		public abstract void Folded();
		public abstract void NoOwner(); // Mid game
		public abstract void WaitingForGame();
    }
}