
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
		private int _playerId = -1;
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

		public void ClaimMat(){

			if( gameState.JoinGame() ){
				Networking.SetOwner(Networking.LocalPlayer, gameObject);
				_playerId = Networking.LocalPlayer.playerId;

				RequestSerialization();
				OnDeserialization();

				SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneClaimedMat");
			}

		}

		public void SomeoneClaimedMat(){
			Log(player.displayName + " joined the game.");
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
