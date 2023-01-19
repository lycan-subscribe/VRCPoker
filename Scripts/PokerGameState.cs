using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{

    // NETWORK CODE HANDLING

    // Players are represented here by GameMats
    public abstract class PokerGameState : UdonSharpBehaviour
    {
        #region Prefabs

		public DealerMat dealerMat;
		public GameMat[] playerMats;

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
		public int currentPlayer = -1; // Index of playerMats whose turn it is

        #endregion


        /*void Start()
		{
			// Validate here? Make sure nothing is null?
		}*/

		public override void OnDeserialization(){
			Log("[DEBUG] Game State Deserialization");
		}


        /*
         *  EVENTS TRIGGERED BY UI
         */

        // Triggered by Dealer Mat
        public bool TriggerStartGame(){
            if( CanStart() ){
				gameInProgress = true;

				if( StartGame() ){
                    SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneStartedGame");
                    return true;
                }
			}

			return false;
        }
        protected abstract bool StartGame();
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

        // Triggered by GameMat Turn UI
        public bool TriggerFold(GameMat mat){
            // assert mat == playerMats[currentPlayer]
			// assert gameInProgress

            return Fold();
        }
        protected abstract bool Fold();
        public bool TriggerCallBetRaise(GameMat mat, int amt){
            // mat == playerMats[currentPlayer]
			// assert gameInProgress

            return CallBetRaise(amt);
        }
        protected abstract bool CallBetRaise(int amt);

        // Triggered by GameMat
        public bool JoinGame(GameMat mat){
            bool alreadyJoined = InGame(Networking.LocalPlayer);

			if(!alreadyJoined && !gameInProgress){
				// Claim the mat
                mat._playerId = Networking.LocalPlayer.playerId;
				Networking.SetOwner(Networking.LocalPlayer, mat.gameObject);
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

        public void EndGame(){
            gameInProgress = false;

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

        public void PlayerWon(){
            SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneStartedGame");
        }


        /*
         *  UTILITY
         */

        public bool InGame(VRCPlayerApi player){
			if(player == null) return false; // Otherwise a null player might return true

			bool alreadyJoined = false;
			foreach(GameMat gm in playerMats){
				if(gm.player == player)
					alreadyJoined = true;
			}

			return alreadyJoined;
		}

        // Gamemats are used as the player array, check them
		protected int NumPlayers(){
			int numJoined = 0;
			
			foreach(GameMat gm in playerMats){
				if(gm.player != null)
					numJoined ++;
			}

			return numJoined;
		}

        protected GameMat MyMat(){
			foreach(GameMat gm in playerMats){
				if(gm.player == Networking.LocalPlayer){
					return gm;
				}
			}
			return null;
		}

        protected void Log(string msg){
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
