using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{
	
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class TexasHoldemGameState : UdonSharpBehaviour
	{

		#region Settings

		//public int maxRounds = 9;

		public DealerMat dealerMat;
		public GameMat[] playerMats;
		public const int startingBet = 5;

		#endregion


		#region Builtin

		public MeshRenderer chip1;
		public MeshRenderer chip5;
		public MeshRenderer chip25;
		public MeshRenderer chip50;
		public MeshRenderer chip100;

		public Logger logger;

		#endregion


		#region GameVariables

		[UdonSynced]
		public bool gameInProgress = false;
		[UdonSynced]
		public int roundNumber = 0;
		[UdonSynced]
		public int currentPlayer = -1; // Index of playerMats whose turn it is
		[UdonSynced]
		public int currentBet = 0;
		[UdonSynced]
		public bool[] playerInRound; // Will be the same size as playerMats

		#endregion
		

		void Start()
		{
			// Validate here? Make sure nothing is null?

			Log("Initializing table...");
			playerInRound = new bool[playerMats.Length];
		}

		public override void OnDeserialization(){
			Log("[DEBUG] Game State Deserialization");
		}



		// Triggered by DealerMat
		public bool StartGame(){
			if( CanStart() ){
				gameInProgress = true;
				roundNumber = -1;

				for(int i=0; i<playerMats.Length; i++){
					if( playerMats[i].player != null ){
						playerInRound[i] = true;
					}
					else{
						playerInRound[i] = false;
					}
				}

				NextRound();
				SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneStartedGame");

				return true;
			}

			return false;
		}

		// Someone started the game
		public void SomeoneStartedGame(){
			Log("Starting game...");
		}

		private void NextRound(){
			roundNumber += 1;
			currentBet = startingBet;
			
			currentPlayer = -1; // Next player is 0

			NextPlayer();
		}

		private void NextPlayer(){
			currentPlayer += 1;

			if( currentPlayer >= playerMats.Length ){
				NextRound();
				return;
			}
			else if( playerMats[currentPlayer].player == null ){ // Check if mat unclaimed, in case
				NextPlayer();
				return;
			}
			else if( playerInRound[currentPlayer] == false){ // Player folded
				NextPlayer();
				return;
			}

			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
			OnDeserialization();

			Networking.SetOwner(Networking.LocalPlayer, dealerMat.gameObject);
			dealerMat.RequestSerialization();
			dealerMat.OnDeserialization();

			foreach(GameMat mat in playerMats){
				Networking.SetOwner(Networking.LocalPlayer, mat.gameObject);
				mat.RequestSerialization();
				mat.OnDeserialization();
			}
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

		// Triggered by GameMat
		public bool Fold(GameMat mat){
			// assert mat == playerMats[currentPlayer]
			// assert gameInProgress

			playerInRound[currentPlayer] = false;
			NextPlayer();
			return true;
		}

		// Triggered by GameMat
		public bool CallBetRaise(GameMat mat, int amount){
			// mat == playerMats[currentPlayer]
			// assert gameInProgress

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

		public bool InGame(VRCPlayerApi player){
			if(player == null) return false; // Otherwise a null player might return true

			bool alreadyJoined = false;
			foreach(GameMat gm in playerMats){
				if(gm.player == player)
					alreadyJoined = true;
			}

			return alreadyJoined;
		}

		private GameMat MyMat(){
			foreach(GameMat gm in playerMats){
				if(gm.player == Networking.LocalPlayer){
					return gm;
				}
			}
			return null;
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