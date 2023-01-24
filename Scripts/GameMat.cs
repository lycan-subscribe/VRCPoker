
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


		[UdonSynced]
		public int _playerId = -1;
		public VRCPlayerApi player = null;

		[UdonSynced]
		public int callBetRaiseAmt = 0;


		public override void OnDeserialization(){
			player = VRCPlayerApi.GetPlayerById(_playerId); // Throws errors that can't be caught with udon??

			if( player == null ){ // Noone has claimed this mat
				Log("[DEBUG] Deserializing empty mat");
				joinButton.gameObject.SetActive(true); // Join if you want
			}
			else{ // Someone claimed this mat
				Log("[DEBUG] Deserializing mat owned by " + player.displayName);
				joinButton.gameObject.SetActive(false); // You can't join
			}

			if( gameState.gameInProgress ){

				if( gameState.playerMats[gameState.currentPlayer] == this ){ // This mat's turn
					if( Networking.LocalPlayer == player ){ // You own the mat
						// Your turn
						Log("[DEBUG] your turn - player " + gameState.currentPlayer);
						turnUI.gameObject.SetActive(true);
					}
					else { // Someone else's mat
						turnUI.gameObject.SetActive(false);
					}
					
				}
				else { // Not this mat's turn
					turnUI.gameObject.SetActive(false);
				}
			}
			else { // No game happening right now
				turnUI.gameObject.SetActive(false);
			}
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

		// Called by the turn UI
		public void Fold(){
			gameState.SendCustomNetworkEvent(NetworkEventTarget.All, "TriggerFold");
		}

		// Called by the turn UI
		public void CallBetRaise(){
			RequestSerialization(); // for callBetRaiseAmt

			gameState.SendCustomNetworkEvent(NetworkEventTarget.All, "TriggerCallBetRaise");
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
