
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker {

	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class DealerMat : UdonSharpBehaviour
	{
		public PokerGameState gameState;
		public StartButton startButton;
		public CardHand cards;


		public void StartGame(){
			if( gameState.TriggerStartGame() ){
				
			}
		}


		// Deserialization from game state

		public void InGame(){
			startButton.gameObject.SetActive(false);
		}

		public void WaitingForPlayers(){
			startButton.gameObject.SetActive(false);
		}

		public void CanStart(){
			startButton.gameObject.SetActive(true);
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