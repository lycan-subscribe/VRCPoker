using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CardHand : UdonSharpBehaviour
    {
        public MeshFilter[] cardObjects;
        public VRCPlayerApi onlyRenderFor = null;

        #region Appearance
        public Mesh[] cardMeshes;
        public Mesh mysteryCard;
        #endregion

        [UdonSynced]
        public Suit[] cardSuits;
        [UdonSynced]
        public Rank[] cardRanks;
        [UdonSynced]
        public int Length;

        void Start()
        {
            // cardObjects is size of the hand, so we can set all arrays to constant size
            cardSuits = new Suit[ cardObjects.Length ];
            cardRanks = new Rank[ cardObjects.Length ];
            Length = 0;

            for(int i=0; i < cardObjects.Length; i++){
                cardSuits[i] = Suit.DNE;
                cardRanks[i] = Rank.DNE;
            }

            OnDeserialization();
        }

        public override void OnDeserialization(){
            
            for(int i=0; i < cardObjects.Length; i++){
                if( cardSuits[i] != Suit.DNE && i < Length ){
                    // Render card
                    cardObjects[i].gameObject.SetActive(true);
                    
                    if( onlyRenderFor == null || Networking.LocalPlayer == onlyRenderFor ){
                        cardObjects[i].mesh = GetCardMesh( cardSuits[i], cardRanks[i] );
                    }
                    else{
                        cardObjects[i].mesh = mysteryCard;
                    }
                }
                else{
                    cardObjects[i].gameObject.SetActive(false);
                }
            }
        }

        // cardMeshes should be:
        // Diamonds Ace-King, Spades Ace-King, Clubs Ace-King, Hearts Ace-King
        // e.g. Ace of Diamonds is 0, Ace of Spades is 13
        private Mesh GetCardMesh(Suit s, Rank r){
            int index = (((int)s-1) * 13) + ((int)r-1);

            return cardMeshes[index];
        }
    }


    public enum Suit{
        DNE,
        Diamonds,
        Spades,
        Clubs,
        Hearts
    }

    public enum Rank{
        DNE,
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }
}