
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using System;

namespace VRCPoker{
	
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class GameMat : UdonSharpBehaviour
	{
		public PokerGameState gameState;
		public JoinButton joinButton;
		public GameObject turnUI;
		public Text foldText;
		public Text callText;
		public GameObject someoneElsesTurnIndicator;
		public CardHand hand;
		public Text debugBetAmt; // Debug - temporary

		public VRCPlayerApi player = null;

		int toBet = 0;


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
		public void SetBet(int bet){
			toBet = bet;
		}

		// Called by the debug bet UI - temporary
		public void IncreaseBet(){
			toBet += 5;
			debugBetAmt.text = toBet.ToString();
		}

		// Called by the debug bet UI - temporary
		public void DecreaseBet(){
			toBet -= 5;
			debugBetAmt.text = toBet.ToString();
		}


		//  Deserialization from game state, player is already correct

		public void MyTurn(){
			turnUI.SetActive(true);
			joinButton.gameObject.SetActive(false);
			someoneElsesTurnIndicator.SetActive(false);
		}

		public void SomeoneElsesTurn(){
			turnUI.SetActive(false);
			joinButton.gameObject.SetActive(false);
			someoneElsesTurnIndicator.SetActive(true);
		}

		public void WaitingForTurn(){
			turnUI.SetActive(false);
			joinButton.gameObject.SetActive(false);
			someoneElsesTurnIndicator.SetActive(false);
		}

		public void Folded(){
			turnUI.SetActive(false);
			joinButton.gameObject.SetActive(false);
			someoneElsesTurnIndicator.SetActive(false);
		}

		public void NoOwner(){ // Mid game
			turnUI.SetActive(false);
			joinButton.gameObject.SetActive(false);
			someoneElsesTurnIndicator.SetActive(false);
		}

		public void WaitingForGame(){
			turnUI.SetActive(false);
			someoneElsesTurnIndicator.SetActive(false);

			if( player == null ){
				joinButton.gameObject.SetActive(true); // Join if you want
			}
			else{
				joinButton.gameObject.SetActive(false); // Already taken
			}
		}


		private void Log(string msg){
			gameState.logger._Log("GameMat", msg);
		}

		// Fix a glitch from 2022
		public override void OnPlayerJoined(VRCPlayerApi _){
			if (Networking.LocalPlayer.IsOwner(gameObject)){
				RequestSerialization();
			}
		}
	}
	
}
