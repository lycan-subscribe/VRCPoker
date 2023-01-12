
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class JoinButton : UdonSharpBehaviour
    {
        public GameMat gameMat;

        public override void Interact(){
            gameMat.ClaimMat();
        }
    }
}