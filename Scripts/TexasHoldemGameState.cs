﻿using UdonSharp;
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
		public const int maxRaisesPerRound = 2;

		#endregion


		#region GameVariables
		
		[UdonSynced]
		public int roundNumber = 0; // Every time a card is dealt to table basically
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
			ShuffleDeck();

			for(int i=0; i<playerMats.Length; i++){
				if( playerMats[i].player != null ){
					playerInGame[i] = true;
					DealCards(playerMats[i].hand, 2);
				}
				else{
					playerInGame[i] = false;
				}
			}

			return true;
		}

		// Is also called at the beginning of the game
		private void RoundFinished(){
			roundNumber++;
			currentBet = 0;
			for(int i=0; i<playerMats.Length; i++){
				playerBet[i] = 0;
			}

			if( roundNumber == 1 ){
				currentBet = startingBet;
			}
			if( roundNumber == 2 ){
				DealCards(dealerMat.cards, 3);
			}
			else if( roundNumber == 3 ){
				DealCards(dealerMat.cards, 1);
			}
			else if( roundNumber == 4){
				DealCards(dealerMat.cards, 1);
			}
			else if( roundNumber >= 5){
				// Make condensed array of hands of players who haven't folded
				CardHand[] playerHands = new CardHand[NotFolded()];
				int index = 0;
				for(int i=0; i<playerMats.Length; i++){
					if( playerInGame[i] ){
						playerHands[index] = playerMats[i].hand;
						index++;
					}
				}

				int winnerIndex = WinningHandSolver.GetWinningHand(dealerMat.cards, playerHands);
				// Convert back to index of playerMats
				int winner = 0;
				for(int i=0; i<playerMats.Length; i++){
					if( playerInGame[i] ){
						if(winnerIndex == index){
							winner = i;
							break;
						}

						index++;
					}
				}

				GiveChips(winner, pot);
				pot = 0;
				EndGame(winner);
				return;
			}

			Log("[DEBUG] Round finished. Starting round " + roundNumber);

			// lastPlayerToRaise stays the same for now
			// End the game if the round number is high enough?
		}

		// Is also called at the beginning of the game
		protected override void NextPlayer(){
			// currentPlayer turn, guaranteed they are still in game

			if( currentPlayer == lastPlayerToRaise ){
				RoundFinished(); // Also if it goes around too many times? No infinite raising
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
			if( NumChips(currentPlayer) < amount ){
				Log("[DEBUG] Not enough chips!");
				return false;
			}
			else if( playerBet[currentPlayer] + amount < currentBet ){
				Log("[DEBUG] Invalid amount; current bet is " + currentBet + " and you've only put in " + playerBet[currentPlayer]);
				return false;
			}
			else if( playerBet[currentPlayer] + amount > currentBet ){ // Raise
				lastPlayerToRaise = currentPlayer;
				currentBet = playerBet[currentPlayer] + amount;
			}

			TakeChips(currentPlayer, amount);
			pot += amount;
			playerBet[currentPlayer] += amount;

			TriggerNextPlayer();

			return true;
		}

		public override int GetMinimumBet(){ //For the current player
			return currentBet - playerBet[currentPlayer];
		}

		
	}

}