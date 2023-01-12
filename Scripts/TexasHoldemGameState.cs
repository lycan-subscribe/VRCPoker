
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

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

		#endregion
		

		void Start()
		{
			// Validate here? Make sure nothing is null?

			logger._Log("GameState", "Initializing table...");
		}

		public bool JoinGame(){
			VRCPlayerApi joining = Networking.LocalPlayer;

			bool alreadyJoined = false;
			foreach(GameMat gm in playerMats){
				if(gm.player == joining)
					alreadyJoined = true;
			}

			return !alreadyJoined && !gameInProgress;
		}
	}

}