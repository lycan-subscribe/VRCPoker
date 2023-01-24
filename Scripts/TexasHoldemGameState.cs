using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{

	// GAME LOGIC

	// All functions in this class are guaranteed to be run by the game owner (person who
	//   pressed start) and will be synced automatically
	// Players are represented here by an integer, where their index is of playerMats
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class TexasHoldemGameState : PokerGameState
	{

		#region Settings

		public const int startingBet = 5;

		#endregion


		#region GameVariables
		
		[UdonSynced]
		public bool[] playerInGame; // Size of gameMat, who is playing & hasn't folded?
		[UdonSynced]
		public int roundNumber = 0;
		[UdonSynced]
		public int currentBet = 0;

		#endregion

		void Start(){
			Log("Initializing table...");
			playerInGame = new bool[playerMats.Length];
		}


		protected override bool StartGame(){
			roundNumber = 0;

			for(int i=0; i<playerMats.Length; i++){
				if( playerMats[i].player != null ){
					playerInGame[i] = true;
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
			if( playerInGame[currentPlayer] == false ){
				TriggerNextPlayer(); // Recursive
				return;
			}

			// currentPlayer turn
		}

		protected override bool Fold(){
			//Log("[DEBUG] folded");

			playerInGame[currentPlayer] = false;

			if( NumPlayersInGame() == 1 ){
				// Only one person left, so they win by default
				for(int i=0; i<playerMats.Length; i++){
					if(playerInGame[i]){
						EndGame(i);
					}
				}
			}
			else{
				TriggerNextPlayer();
			}

			return true;
		}

		protected override bool CallBetRaise(int amount){
			//Log("[DEBUG] Called");

			TriggerNextPlayer(); // As a test

			return true;
		}


		// Utility

		// How many haven't folded yet
		private int NumPlayersInGame(){
			int numPlaying = 0;

			foreach(bool p in playerInGame){
				if(p) numPlaying++;
			}

			return numPlaying;
		}

		
	}

}