
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCPoker{
	
	public class TexasHoldemGameState : UdonSharpBehaviour
	{
		public MeshRenderer chip1;
		public MeshRenderer chip5;
		public MeshRenderer chip25;
		public MeshRenderer chip50;
		public MeshRenderer chip100;
		
		public DealerMat dealerMat;
		public GameMat[] playerMats;
		
		public Logger logger;
		
		void Start()
		{
			
		}
	}

}