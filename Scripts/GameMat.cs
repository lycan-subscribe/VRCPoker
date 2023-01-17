
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{
	
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class GameMat : UdonSharpBehaviour
	{
		public TexasHoldemGameState gameState;
		public JoinButton joinButton;


		[UdonSynced]
		public int _playerId = -1;
		public VRCPlayerApi player = null;


		public override void OnDeserialization(){
			player = VRCPlayerApi.GetPlayerById(_playerId);
			if( player == null ){ // Noone has claimed this mat
				joinButton.gameObject.SetActive(true); // Join if you want
			}
			else{ // Someone claimed this mat
				joinButton.gameObject.SetActive(false); // You can't join
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

		public void Fold(){
			Log("Folded");
		}

		public void CallBetRaise(){
			Log("Called");
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
