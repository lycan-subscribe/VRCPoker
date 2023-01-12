
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

		#endregion
		
		public Logger logger;
		
		void Start()
		{
			// Validate here? Make sure nothing is null?

			logger._Log("GameState", "Initializing table...");
		}
	}

}