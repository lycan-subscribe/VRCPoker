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
		public int currentBet = 0; // Reset every time the round ends
		[UdonSynced]
		public int lastPlayerToRaise = 0;
		[UdonSynced]
		public int pot = 0;
		[UdonSynced]
		public int[] playerBet;

		#endregion

		void Start(){
			base.BaseStart();
			Log("Initializing table...");

			playerBet = new int[playerMats.Length];
		}

		protected override void AfterDeserialization(){
			dealerMat.debugPotAmt.text = pot.ToString();
		}


		protected override bool StartGame(){
			roundNumber = 0;
			currentBet = startingBet;
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

		void RoundFinished(){
			roundNumber++;

			Log("[DEBUG] Round finished. Starting round " + roundNumber);

			// lastPlayerToRaise stays the same for now
			// End the game if the round number is high enough?
		}

		protected override void NextPlayer(){
			// currentPlayer turn, guaranteed they are still in game

			if( currentPlayer == lastPlayerToRaise ){
				RoundFinished();
			}
		}

		protected override bool Fold(){
			//Log("[DEBUG] folded");

			playerInGame[currentPlayer] = false;
			TriggerNextPlayer();

			return true;
		}

		protected override bool CallBetRaise(int amount){
			//Log("[DEBUG] Called");
			if( NumChips(currentPlayer) < amount ) return false;

			TakeChips(currentPlayer, amount);
			pot += amount;
			lastPlayerToRaise = currentPlayer;

			TriggerNextPlayer();

			return true;
		}

		protected override int GetMinimumBet(){
			return currentBet;
		}

		
	}

}