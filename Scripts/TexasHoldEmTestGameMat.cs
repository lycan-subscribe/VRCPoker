using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using System;

namespace VRCPoker{
	
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class TexasHoldEmTestGameMat : GameMat
	{
		public Color VALID_TEXT = Color.white;
		public Color INVALID_TEXT = Color.grey;

		public TexasHoldemGameState gameState;
		public TestJoinButton joinButton;
		public GameObject turnUI;
		public Text foldText;
		public Text callText;
		public GameObject someoneElsesTurnIndicator;
		public GameObject leaveButton;
		
		public Text debugBetAmt; // Debug - temporary

		int toBet = 0;


		public override void ResetMat(){
			toBet = 0;
		}

		// Called in the lobby before the game starts, when someone wants the mat
		public void ClaimMat(){

			if( gameState.JoinGame(this) ){
				SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneClaimedMat");
			}

		}

		public void SomeoneClaimedMat(){
			Log(player.displayName + " joined the game.");
		}


		//  UI Calls

		// Leave btn
		public void LeaveGame(){
			gameState.LeaveGame(player);
		}

		// Called by the turn UI
		public void Fold(){
			if( gameState.TriggerFold(this) ){
				
			}
		}

		// Called by the turn UI
		public void CallBetRaise(){
			gameState.TriggerCallBetRaise(this, toBet);
		}

		// Called by the bet UI (eventually?)
		public void SetNumberOfChipsToBet(int bet){
			toBet = bet;
		}

		// Called by the debug bet UI - temporary
		public void IncreaseBet(){
			toBet += 5;
			UpdateDebugUI();
		}

		// Called by the debug bet UI - temporary
		public void DecreaseBet(){
			toBet -= 5;
			UpdateDebugUI();
		}

		private void UpdateDebugUI(){
			debugBetAmt.text = toBet.ToString();
			int amountNeeded = gameState.GetMinimumBet(); // Minimum you need to put in to call
			if(toBet > amountNeeded){
				callText.text = "Raise";
				callText.color = VALID_TEXT;
			}
			else if(toBet == amountNeeded && toBet == 0){
				callText.text = "Check";
				callText.color = VALID_TEXT;
			}
			else if(toBet == amountNeeded){
				callText.text = "Call";
				callText.color = VALID_TEXT;
			}
			else{
				callText.text = "(Min " + amountNeeded + ")";
				callText.color = INVALID_TEXT;
			}
		}


		//  Deserialization from game state, player is already correct

		public override void GameStateChanged(bool gameInProgress, bool thisMatsTurn, bool folded){
			if( !gameInProgress && player == null ){ // Join if you want
				joinButton.gameObject.SetActive(true);
			}
			else{ // Already taken
				joinButton.gameObject.SetActive(false);
			}
			
			if( player == Networking.LocalPlayer && thisMatsTurn ){ // Your turn
				turnUI.SetActive(true);
				UpdateDebugUI();
			}
			else{
				turnUI.SetActive(false);
			}

			if( player != Networking.LocalPlayer && thisMatsTurn ){ // Someone else's turn
				someoneElsesTurnIndicator.SetActive(true);
			}
			else{
				someoneElsesTurnIndicator.SetActive(false);
			}

			if( player == Networking.LocalPlayer ){ // Able to leave
				leaveButton.SetActive(true);
			}
			else{
				leaveButton.SetActive(false);
			}
		}


		private void Log(string msg){
			gameState.logger._Log("TexasHoldEmGameMat", msg);
		}

		// Fix a glitch from 2022
		public override void OnPlayerJoined(VRCPlayerApi _){
			if (Networking.LocalPlayer.IsOwner(gameObject)){
				RequestSerialization();
			}
		}
	}
	
}
