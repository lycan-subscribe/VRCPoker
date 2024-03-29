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
		
		private int previousPlayer = -1; // Hack to make call/check/raise events work
		[UdonSynced]
		public MOVE_TYPE lastMove = MOVE_TYPE.NONE; // Hack to make call/check/raise events work
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
			OnDeserialization();
		}

		protected override void AfterDeserialization(){
			dealerMat.debugPotAmt.text = pot.ToString();

			// EVENTS
			if(currentPlayer != previousPlayer){
				if(lastMove == MOVE_TYPE.NONE){
					// Start of game
				}
				else if(lastMove == MOVE_TYPE.RAISE){
					playerMats[previousPlayer].TextParticle( "Raise to " + currentBet );
				}
				else if(lastMove == MOVE_TYPE.CHECK){
					playerMats[previousPlayer].TextParticle( "Check" );
				}
				else if(lastMove == MOVE_TYPE.CALL){
					playerMats[previousPlayer].TextParticle( "Call" );
				}
				else if(lastMove == MOVE_TYPE.FOLD){
					playerMats[previousPlayer].TextParticle( "Fold" );
				}

				previousPlayer = currentPlayer;
			}

			PrintAllVariables(); // DEBUG
		}


		protected override bool StartGame(){
			roundNumber = -1;
			lastMove = MOVE_TYPE.NONE;
			ShuffleDeck();

			ClearHand(dealerMat.cards);
			for(int i=0; i<playerMats.Length; i++){
				if( playerMats[i].player != null ){
					playerInGame[i] = true;
					ClearHand(playerMats[i].hand);
					DealCards(playerMats[i].hand, 2);
				}
				else{
					playerInGame[i] = false;
				}
				playerWon[i] = false;
			}

			RoundFinished();

			return true;
		}

		// Also runs at the beginning of the game with round 0
		private void RoundFinished(){
			roundNumber++;
			currentBet = 0;
			lastPlayerToRaise = -1;
			for(int i=0; i<playerMats.Length; i++){
				playerBet[i] = 0;

				if( playerInGame[i] == true ){
					if( lastPlayerToRaise == -1 )
						lastPlayerToRaise = i; // set lastPlayerToRaise to the first player still in game
				}
			}

			if( roundNumber == 1 ){ // Beginning of the game
				currentBet = startingBet;
			}
			else if( roundNumber == 2 ){
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

				winMessage = "";
				int[] winnerIndices = WinningHandSolver.GetWinningHands(dealerMat.cards, playerHands, ref winMessage);
				// Convert back to index of playerMats
				index = 0;
				int winnerIndex = 0;
				for(int i=0; i<playerMats.Length; i++){
					if( playerInGame[i] ){
						if(index == winnerIndices[winnerIndex]){
							playerWon[i] = true;

							winnerIndex++;
							if(winnerIndex >= winnerIndices.Length) break;
						}

						index++;
					}
				}

				TriggerEndGame();
				return;
			}

			Log("[DEBUG] Round finished. Starting round " + roundNumber);

			// lastPlayerToRaise stays the same for now
			// End the game if the round number is high enough?
		}

		protected override void EndGame(){
			int numPlayersWhoWon = 0;
			for(int i=0; i<playerMats.Length; i++){
				if( playerWon[i] ) numPlayersWhoWon ++;
			}

			for(int i=0; i<playerMats.Length; i++){
				if( playerWon[i] ){
					GiveChips(i, pot / numPlayersWhoWon);
				}
			}

			pot = 0;
		}

		// Is also called at the beginning of the game
		protected override void NextPlayer(){
			// currentPlayer turn, guaranteed they are still in game

			if( !playerInGame[lastPlayerToRaise] ){ // Happens if they left mid game
				RoundFinished();
			}
			if( currentPlayer == lastPlayerToRaise ){
				RoundFinished(); // Also if it goes around too many times? No infinite raising
			}
		}

		protected override bool Fold(){
			//Log("[DEBUG] folded");

			playerInGame[currentPlayer] = false;
			lastMove = MOVE_TYPE.FOLD;
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
				lastMove = MOVE_TYPE.RAISE;
			}
			else if( amount == 0 ){ // Check
				lastMove = MOVE_TYPE.CHECK;
			}
			else{ // Call
				lastMove = MOVE_TYPE.CALL;
			}

			TakeChips(currentPlayer, amount);
			pot += amount;
			playerBet[currentPlayer] += amount;

			TriggerNextPlayer();

			return true;
		}

		public int GetMinimumBet(){ //For the current player
			return currentBet - playerBet[currentPlayer];
		}

		public bool CanCall(int amt){
			return amt == GetMinimumBet();
		}

		public bool CanRaise(int amt){
			return amt > GetMinimumBet();
		}


		private void PrintAllVariables(){
			string toLog = "[DEBUG] All game variables: \n"
				+ "playerMatOwners playerInGame playerBet playerWon numPlayerChips\n";
			for(int i=0; i<playerMats.Length; i++){
				toLog += playerMatOwners[i] + " "
					   + playerInGame[i] + " "
					   + playerBet[i] + " " // errors?
					   + playerWon[i] + " "
					   + numPlayerChips[i] + "\n";
			}

			toLog += "gameInProgress: " + gameInProgress + "\n";
			toLog += "previousPlayer: " + previousPlayer + "\n";
			toLog += "currentPlayer: " + currentPlayer + "\n";
			toLog += "lastMove: " + lastMove + "\n";
			toLog += "drawNext: " + drawNext + "\n";
			toLog += "roundNumber: " + roundNumber + "\n";
			toLog += "currentBet: " + currentBet + "\n";
			toLog += "lastPlayerToRaise: " + lastPlayerToRaise + "\n";
			toLog += "pot: " + pot + "\n";

			Log(toLog);

		}
	}

	public enum MOVE_TYPE {
		NONE,
		FOLD,
		CHECK,
		CALL,
		RAISE
	}

}