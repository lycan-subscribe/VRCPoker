using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace VRCPoker{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CardHand : UdonSharpBehaviour
    {
        public MeshFilter[] cardObjects;
        VRCPlayerApi onlyRenderFor = null;

        #region Appearance
        public Mesh[] cardMeshes;
        public Mesh mysteryCard;
        #endregion

        [UdonSynced]
        Suit[] cardSuits;
        [UdonSynced]
        Rank[] cardRanks;

        void Start()
        {
            // cardObjects is size of the hand, so we can set all arrays to constant size
            cardSuits = new Suit[ cardObjects.Length ];
            cardRanks = new Rank[ cardObjects.Length ];

            for(int i=0; i < cardObjects.Length; i++){
                cardSuits[i] = Suit.DNE;
                cardRanks[i] = Rank.DNE;
            }

            OnDeserialization();
        }

        public override void OnDeserialization(){
            
            for(int i=0; i < cardObjects.Length; i++){
                if( cardSuits[i] != Suit.DNE ){
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
            int index = ((int)s * 13) + (int)r;

            return cardMeshes[index];
        }
    }


    public enum Suit{
        DNE = -1,
        Diamonds,
        Spades,
        Clubs,
        Hearts
    }

    public enum Rank{
        DNE = -1,
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