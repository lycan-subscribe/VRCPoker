
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker {
	
	public class DealerMat : UdonSharpBehaviour
	{
		public PokerGameState gameState;
		public StartButton startButton;
		
		void Start(){
			OnDeserialization();
		}

		public void StartGame(){
			if( gameState.TriggerStartGame() ){
				Networking.SetOwner(Networking.LocalPlayer, gameObject);
				RequestSerialization();
				//OnDeserialization();
			}
		}

		public override void OnDeserialization(){
			Log("[DEBUG] Deserialization");
			if( gameState.CanStart() ){
				startButton.gameObject.SetActive(true);
			}
			else{
				startButton.gameObject.SetActive(false);
			}
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