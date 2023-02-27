﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
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

		[UdonSynced]
		public int[] playerMatOwners; // Size of playerMats, player API IDs, -1 if noone


        #region GameVariables

        [UdonSynced]
		public bool gameInProgress = false;
        [UdonSynced]
		public int currentPlayer = -1; // Index of playerMats whose turn it is
		[UdonSynced]
		public bool[] playerInGame; // Size of playerMats, who is playing & hasn't folded?
		[UdonSynced]
		public bool[] playerWon; // Size of playerMats, used after round ends
		[UdonSynced]
		public int[] numPlayerChips;

		// Deck Variables
		[UdonSynced]
		public Suit[] deckSuits = new Suit[52];
		[UdonSynced]
		public Rank[] deckRanks = new Rank[52];
		[UdonSynced]
		public int drawNext = 51; // Start at the end

        #endregion


        protected void BaseStart()
		{
			// Validate here? Make sure nothing is null?

			playerMatOwners = new int[playerMats.Length];
			playerInGame = new bool[playerMats.Length];
			numPlayerChips = new int[playerMats.Length];
			playerWon = new bool[playerMats.Length];

			for(int i=0; i<deckRanks.Length; i++){
				deckRanks[i] = (Rank) ( i % 13 );
				deckSuits[i] = (Suit) ( i / 13 );
			}

			OnDeserialization();
		}

		public override void OnDeserialization(){
			Log("[DEBUG] Game State Deserialization");

			for(int i=0; i<playerMats.Length; i++){
				GameMat mat = playerMats[i];
				mat.player = VRCPlayerApi.GetPlayerById(playerMatOwners[i]);

				if( gameInProgress ){
					mat.hand.onlyRenderFor = mat.player;

					if( mat.player == null ){ // Noone owns this mat
						mat.NoOwner();
					}
					else if( playerMats[currentPlayer] == mat ){ // This mat's turn
						if( Networking.LocalPlayer == mat.player ){ // You own the mat
							Log("[DEBUG] your turn - player " + currentPlayer);
							mat.MyTurn();
						}
						else { // Someone else's mat
							mat.SomeoneElsesTurn();
						}
						
					}
					else { // Not this mat's turn
						mat.WaitingForTurn();
					}
				}
				else{
					mat.hand.onlyRenderFor = null; // Show cards at the end
					mat.WaitingForGame();
				}
			}

			if( CanStart() ){
				dealerMat.CanStart();
			}
			else if( gameInProgress ){
				dealerMat.InGame();
			}
			else{
				dealerMat.WaitingForPlayers();
			}

			AfterDeserialization();
		}
		protected abstract void AfterDeserialization();


        /*
         *  EVENTS TRIGGERED BY UI
         */

        // Triggered by Dealer Mat
        public bool TriggerStartGame(){
            if( CanStart() ){
				gameInProgress = true;

				if( StartGame() ){
                    SendCustomNetworkEvent(NetworkEventTarget.All, "SomeoneStartedGame");
					currentPlayer = -1;
					TriggerNextPlayer(); // Serializes
                    
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
				playerMatOwners[ MatIndex(mat) ] = Networking.LocalPlayer.playerId;
				SerializeAll();

				return true;
			}

			return false;
        }

        public void EndGame(){
            gameInProgress = false;

			SerializeAll();

            SendCustomNetworkEvent(NetworkEventTarget.All, "PlayersWon");
        }

        public void PlayersWon(){
            // ...
        }

		// Is also called at the beginning of the game
		public void TriggerNextPlayer(){
			if( NotFolded() == 1 ){
				// Only one person left, so they win by default
				for(int i=0; i<playerMats.Length; i++){
					if(playerInGame[i]){
						EndGame();
					}
				}
				return;
			}

			currentPlayer++;

			// Find the next gameMat with a player
			while( playerMats[currentPlayer].player == null
				   || playerInGame[currentPlayer] == false ){

				currentPlayer++;
				if( currentPlayer >= playerMats.Length ) break;
			}
			
			if( currentPlayer >= playerMats.Length ){
				currentPlayer = 0;
			}

			// Find the next gameMat with a player again
			while( playerMats[currentPlayer].player == null
				   || playerInGame[currentPlayer] == false ){
				
				currentPlayer++;
				if( currentPlayer >= playerMats.Length ) break; // Should never happen
			}
			//assert currentPlayer < playerMats.Length

			NextPlayer();

			SerializeAll();
		}
		protected abstract void NextPlayer();
		public abstract int GetMinimumBet();


		/*
		 *  DECK
		 */

		// Really hoping Udon 2 comes out soon
		protected void ShuffleDeck(){
			drawNext = deckRanks.Length-1; // End of the deck

			for (int i = deckRanks.Length-1; i > 0; i--) 
			{
				int j = (int) (Random.Range(0,0.9999f) * (i+1)); // Index to swap with, from 0 to i

				Rank r = deckRanks[j];
				Suit s = deckSuits[j];
				deckRanks[j] = deckRanks[i];
				deckSuits[j] = deckSuits[i];
				deckRanks[i] = r;
				deckSuits[i] = s;
			}
		}

		protected void DealCards(CardHand hand, int num){
			
			for(int i=0; i<num; i++){

				hand.cardSuits[hand.Length] = deckSuits[drawNext];
				hand.cardRanks[hand.Length] = deckRanks[drawNext];
				hand.Length ++;
				drawNext --;

			}

			Networking.SetOwner(Networking.LocalPlayer, hand.gameObject);
			hand.RequestSerialization();
			hand.OnDeserialization();
		}

		protected void ClearHand(CardHand hand){

			/*for(int i=0; i<hand.cardSuits.Length; i++){
				// Put them back in the deck? Not going to for now
				hand.cardSuits[i] = Suit.DNE;
				hand.cardRanks[i] = Rank.DNE;
			}*/
			hand.Length = 0;

			Networking.SetOwner(Networking.LocalPlayer, hand.gameObject);
			hand.RequestSerialization();
			hand.OnDeserialization();
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

		// How many haven't folded yet
		public int NotFolded(){
			int numPlaying = 0;

			foreach(bool p in playerInGame){
				if(p) numPlaying++;
			}

			return numPlaying;
		}

        // Gamemats are used as the player array, check them
		public int NumPlayers(){
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

		protected int MatIndex(GameMat mat){
			for(int i=0; i< playerMats.Length; i++){
				if(playerMats[i] == mat)
					return i;
			}
			return -1;
		}

		protected int NumChips(int player){
			return 10000000; //todo
		}

		protected void TakeChips(int player, int amt){
			//todo
		}

		protected void GiveChips(int player, int amt){
			//todo
		}

		// Refresh game state, dealer mat, and all game mats for every player
		private void SerializeAll(){
			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
			OnDeserialization();
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

		public override void OnPlayerLeft(VRCPlayerApi p){
			if( Networking.LocalPlayer.IsOwner(gameObject) ){ // Errors, not sure for who
				for(int i=0; i<playerMats.Length; i++){
					if( playerMats[i].player == p ){
						playerMatOwners[i] = -1;
						playerInGame[i] = false;

						if( i == currentPlayer )
							TriggerNextPlayer();
						else
							SerializeAll();

						ClearHand(playerMats[i].hand);

						break;
					}
				}
			}
		}
    }
}
