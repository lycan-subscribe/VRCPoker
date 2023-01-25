
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using TMPro;
using System;

namespace VRCPoker{
	
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class GameMat : UdonSharpBehaviour
	{
		public PokerGameState gameState;
		public JoinButton joinButton;
		public Canvas turnUI;
		public TMP_Text foldText;
		public TMP_Text callText;
		public CardHand hand;

		public VRCPlayerApi player = null;


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
			gameState.TriggerCallBetRaise(this, 0); // Todo
		}


		//  Deserialization from game state, player is already correct

		public void MyTurn(){
			turnUI.gameObject.SetActive(true);
			joinButton.gameObject.SetActive(false);
		}

		public void SomeoneElsesTurn(){
			turnUI.gameObject.SetActive(false);
			joinButton.gameObject.SetActive(false);
		}

		public void WaitingForTurn(){
			turnUI.gameObject.SetActive(false);
			joinButton.gameObject.SetActive(false);
		}

		public void Folded(){
			turnUI.gameObject.SetActive(false);
			joinButton.gameObject.SetActive(false);
		}

		public void WaitingForGame(){
			turnUI.gameObject.SetActive(false);

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
