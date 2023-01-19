using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{

	// GAME LOGIC

	// All functions in this class are run on the game master's client (player who hits start)
	// Players are represented here by an integer, where their index is the gameMat
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
			roundNumber = -1;

			for(int i=0; i<playerMats.Length; i++){
				if( playerMats[i].player != null ){
					playerInGame[i] = true;
				}
				else{
					playerInGame[i] = false;
				}
			}

			NextRound();

			return true;
		}

		private void NextRound(){
			roundNumber += 1;

			currentPlayer = -1; // Next player is 0
			NextPlayer();
		}

		private void NextPlayer(){
			currentPlayer += 1;

			if( currentPlayer >= playerMats.Length ){
				NextRound();
				return;
			}
			else if( playerMats[currentPlayer].player == null ){ // Check if mat unclaimed, in case
				NextPlayer();
				return;
			}
			else if( playerInGame[currentPlayer] == false){ // Player folded
				NextPlayer();
				return;
			}

			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
			OnDeserialization();

			Networking.SetOwner(Networking.LocalPlayer, dealerMat.gameObject);
			dealerMat.RequestSerialization();
			dealerMat.OnDeserialization();

			foreach(GameMat mat in playerMats){
				Networking.SetOwner(Networking.LocalPlayer, mat.gameObject);
				mat.RequestSerialization();
				mat.OnDeserialization();
			}
		}

		protected override bool Fold(){
			playerInGame[currentPlayer] = false;

			if( NumPlayersInGame() == 1 ){
				// Only one person left, so they win by default
				
			}
			else{
				NextPlayer();
			}

			return true;
		}

		protected override bool CallBetRaise(int amount){
			return false;
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