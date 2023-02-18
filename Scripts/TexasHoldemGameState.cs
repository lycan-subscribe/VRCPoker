using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{

	// GAME LOGIC

	// All functions in this class are guaranteed to be run by the object network owner.
	//   and will be synced automatically
	// Players are represented here by an integer, where their index is of playerMats
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class TexasHoldemGameState : PokerGameState
	{

		#region Settings

		public const int startingBet = 5;

		#endregion


		#region GameVariables
		
		[UdonSynced]
		public int roundNumber = 0;
		[UdonSynced]
		public int currentBet = 0;

		#endregion

		void Start(){
			base.BaseStart();
			Log("Initializing table...");
		}


		protected override bool StartGame(){
			roundNumber = 0;
			ShuffleDeck();

			for(int i=0; i<playerMats.Length; i++){
				if( playerMats[i].player != null ){
					playerInGame[i] = true;
					DealCards(i, 2);
				}
				else{
					playerInGame[i] = false;
				}
			}

			return true;
		}

		protected override void RoundFinished(){
			roundNumber++;

			Log("[DEBUG] Round finished. Starting round " + roundNumber);
		}

		protected override void NextPlayer(){
			// currentPlayer turn, guaranteed they are still in game
		}

		protected override bool Fold(){
			//Log("[DEBUG] folded");

			playerInGame[currentPlayer] = false;
			TriggerNextPlayer();

			return true;
		}

		protected override bool CallBetRaise(int amount){
			//Log("[DEBUG] Called");

			TriggerNextPlayer(); // As a test

			return true;
		}

		
	}

}