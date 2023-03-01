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

        public abstract void ResetMat();
        public abstract void GameStateChanged(bool gameInProgress, bool thisMatsTurn, bool folded);
    }
}