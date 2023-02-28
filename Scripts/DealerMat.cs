
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker {

	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class DealerMat : UdonSharpBehaviour
	{
		public PokerGameState gameState;
		public StartButton startButton;
		public CardHand cards;

		public Text debugPotAmt; // Debug - Temporary


		public void StartGame(){
			if( gameState.TriggerStartGame() ){
				
			}
		}


		// Deserialization from game state

		public void InGame(){
			startButton.gameObject.SetActive(false);
			debugPotAmt.gameObject.SetActive(true); // DEBUG
		}

		public void WaitingForPlayers(){
			startButton.gameObject.SetActive(false);
			debugPotAmt.gameObject.SetActive(false); // DEBUG
		}

		public void CanStart(){
			startButton.gameObject.SetActive(true);
			debugPotAmt.gameObject.SetActive(false); // DEBUG
		}


		private void Log(string msg){
			gameState.logger._Log("DealerMat", msg);
		}

		// Fix a glitch from 2022
		public override void OnPlayerJoined(VRCPlayerApi _){
			if (Networking.LocalPlayer.IsOwner(gameObject)){
				RequestSerialization();
			}
		}
		
	}
	
}