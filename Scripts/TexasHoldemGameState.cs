using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{
	
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class TexasHoldemGameState : UdonSharpBehaviour
	{
		#region Prefabs

		/*
         * Prefabs
         */

		public MeshRenderer chip1;
		public MeshRenderer chip5;
		public MeshRenderer chip25;
		public MeshRenderer chip50;
		public MeshRenderer chip100;
		
		public DealerMat dealerMat;
		public GameMat[] playerMats;

		public Logger logger;

		#endregion


		#region GameVariables

		[UdonSynced]
		public bool gameInProgress = false;
		[UdonSynced]
		public int roundNumber = 0;
		[UdonSynced]
		public int currentPlayer = 0;

		#endregion
		

		void Start()
		{
			// Validate here? Make sure nothing is null?

			logger._Log("GameState", "Initializing table...");
		}

		public override void OnDeserialization(){

		}

		// Triggered by GameMat
		public bool JoinGame(GameMat mat){
			bool alreadyJoined = InGame(Networking.LocalPlayer);

			if(!alreadyJoined && !gameInProgress){
				// Claim the mat
				Networking.SetOwner(Networking.LocalPlayer, mat.gameObject);
				mat._playerId = Networking.LocalPlayer.playerId;
				mat.RequestSerialization();
				mat.OnDeserialization();

				// Refresh the start button in case people can play now
				Networking.SetOwner(Networking.LocalPlayer, dealerMat.gameObject);
				dealerMat.RequestSerialization();
				dealerMat.OnDeserialization();

				return true;
			}

			return false;
		}

		// Triggered by DealerMat
		public bool StartGame(){
			if( CanStart() ){
				gameInProgress = true;
				roundNumber = 0;
				currentPlayer = 0;

				Networking.SetOwner(Networking.LocalPlayer, gameObject);
				RequestSerialization();
				OnDeserialization();
				SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneStartedGame");

				return true;
			}

			return false;
		}

		// Someone started the game
		public void SomeoneStartedGame(){
			Log("Starting game...");
		}

		// Checked by DealerMat
		public bool CanStart(){

			if( gameInProgress ){
				return false;
			}
			else if( NumPlayers() >= 2 ){
				return true;
			}

			return false;
		}


		// Gamemats are used as the player array, check them
		private int NumPlayers(){
			int numJoined = 0;
			
			foreach(GameMat gm in playerMats){
				if(gm.player != null)
					numJoined ++;
			}

			return numJoined;
		}

		private bool InGame(VRCPlayerApi player){
			bool alreadyJoined = false;
			foreach(GameMat gm in playerMats){
				if(gm.player == player)
					alreadyJoined = true;
			}

			return alreadyJoined;
		}

		private void Log(string msg){
			logger._Log("GameCode", msg);
		}

		// Fix a glitch from 2022
		public override void OnPlayerJoined(VRCPlayerApi _){
			if (Networking.LocalPlayer.IsOwner(gameObject)){
				RequestSerialization();
			}
		}
	}

}