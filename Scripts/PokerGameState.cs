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

		// Deck variables
		// e.g. the 0th card in the deck has suit deckSuits[0] and rank deckRanks[0]
		Suit[] deckSuits = new Suit[52];
		Rank[] deckRanks = new Rank[52];
		int nextToDraw = 51; // Draw starting from the end

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

        // Triggered directly by Dealer Mat
        public bool TriggerStartGame(){
            if( CanStart() ){
				if( Networking.IsOwner(gameObject) ){
					gameInProgress = true;

					if( StartGame() ){
						SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneStartedGame");
						currentPlayer = -1;

						// Person who presses the button takes over the game
						Networking.SetOwner(Networking.LocalPlayer, gameObject);
						Networking.SetOwner(Networking.LocalPlayer, dealerMat.gameObject);
						TriggerNextPlayer(); // Serializes
						
						return true;
					}
				}
			}

			return false;
        }
        protected abstract bool StartGame();
        public void SomeoneStartedGame(){
			Log("Starting game...");
		}

		// Triggered directly by GameMat
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
        public void TriggerFold(){
			// assert gameInProgress

			if( Networking.IsOwner(gameObject) )
            	Fold();
        }
        protected abstract bool Fold();

		// Triggered by GameMat Turn UI
        public void TriggerCallBetRaise(){
			// assert gameInProgress

			if( Networking.IsOwner(gameObject) )
            	CallBetRaise( playerMats[currentPlayer].callBetRaiseAmt );
        }
        protected abstract bool CallBetRaise(int amt);

        

        protected void EndGame(int winner){
            gameInProgress = false;
            currentPlayer = winner;

			RequestSerialization();
			OnDeserialization();
			dealerMat.RequestSerialization();
			dealerMat.OnDeserialization();
			SendCustomNetworkEvent(NetworkEventTarget.All, "TurnEnded");
            SendCustomNetworkEvent(NetworkEventTarget.All, "PlayerWon");
        }

        public void PlayerWon(){
            Log(playerMats[currentPlayer].player.displayName + " won the game!");
        }

		// Trigger 
		protected void TriggerNextPlayer(){
			// assert Networking.IsOwner(gameObject)

			currentPlayer++;

			// Find the next gameMat with a player
			while( currentPlayer < playerMats.Length && playerMats[currentPlayer].player == null ){
				currentPlayer++;
			}
			
			if( currentPlayer >= playerMats.Length ){
				RoundFinished();
				currentPlayer = 0;
			}

			// Find the next gameMat with a player again
			while( currentPlayer < playerMats.Length && playerMats[currentPlayer].player == null ){
				currentPlayer++;
			}
			//assert currentPlayer < playerMats.Length

			NextPlayer();

			RequestSerialization();
			OnDeserialization();
			dealerMat.RequestSerialization();
			dealerMat.OnDeserialization();
			SendCustomNetworkEvent(NetworkEventTarget.All, "TurnEnded");
		}
		protected abstract void RoundFinished(); // Called before NextPlayer at the end of one circle
		protected abstract void NextPlayer();
		public void TurnEnded(){
			Log("[DEBUG] turn ended");
			GameMat mat = MyMat();
			if(mat != null){
				mat.OnDeserialization();
			}
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

		// Compare all hands with dealer's hand using traditional poker rules
		// Returns the index of the player in playerMats
		protected int GetWinningHand(){

			return CompareHands.GetWinner(ref dealerMat.cards, ref playerMats);
		}

		// Refresh game state, dealer mat, and all game mats for every player
		/*private void SerializeAll(){
			RequestSerialization();
			OnDeserialization();
			dealerMat.RequestSerialization();
			dealerMat.OnDeserialization();
			SendCustomNetworkEvent(NetworkEventTarget.All, "TurnEnded");
		}*/

        protected void Log(string msg){
			logger._Log("GameCode", msg);
		}

		public override void OnOwnershipTransferred(VRCPlayerApi p){
			Log(p.displayName + " became the host.");
		}

		public override void OnPlayerLeft(VRCPlayerApi p){
			
		}

		// Fix a glitch from 2022
		public override void OnPlayerJoined(VRCPlayerApi _){
			if (Networking.LocalPlayer.IsOwner(gameObject)){
				RequestSerialization();
			}
		}
    }
}
