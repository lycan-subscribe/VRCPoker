
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker{

    // GAME LOGIC

	// All functions in this class are guaranteed to be run by the object network owner.
	//   and will be synced automatically
	// Players are represented here by an integer, where their index is of playerMats
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

    public class BlackjackGameState : PokerGameState
    {
        protected override bool StartGame(){
            return false;
        }

        protected override void AfterDeserialization(){

        }

        protected override bool Fold(){
            return false;
        }

        protected override bool CallBetRaise(int amt){
            return false;
        }

        protected override void NextPlayer(){

        }

        protected override int GetMinimumBet(){
            return 0;
        }
    }

}