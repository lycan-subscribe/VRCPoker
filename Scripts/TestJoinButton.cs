
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestJoinButton : UdonSharpBehaviour
    {
        public TexasHoldEmTestGameMat gameMat;

        public override void Interact(){
            gameMat.ClaimMat();
        }
    }
}