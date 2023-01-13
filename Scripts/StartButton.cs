
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StartButton : UdonSharpBehaviour
    {
        public DealerMat dealerMat;

        public override void Interact(){
            dealerMat.StartGame();
        }
    }
}