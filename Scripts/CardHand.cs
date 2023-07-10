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
            
            if( cardSuits.Length == cardObjects.Length && cardRanks.Length == cardObjects.Length ){ // Due to a funny hack sometimes OnDeserialization runs before Start (hehe)
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
        }

        // cardMeshes should be:
        // Diamonds Ace-King, Spades Ace-King, Clubs Ace-King, Hearts Ace-King
        // e.g. Ace of Diamonds is 0, Ace of Spades is 13
        private Mesh GetCardMesh(Suit s, Rank r){
            int suit = (int) s - 1;
            int rank = (int) r;
            if( r == Rank.Ace ) rank = 0;

            int index = (suit * 13) + rank;

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
        King,
        Ace
    }

    public static class SuitMethods{
        public static string GetName(this Suit s){
            string[] suit_names = { // .ToString() completely breaks during udon runtime, no idea why
                "DNE",
                "diamonds",
                "spades",
                "clubs",
                "hearts"
            };
            return suit_names[(int)s];
        }
    }

    public static class RankMethods{
        public static string GetName(this Rank r){
            string[] rank_names = { // .ToString() completely breaks during udon runtime, no idea why
                "DNE",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
                "nine",
                "ten",
                "jack",
                "queen",
                "king",
                "ace"
            };
            return rank_names[(int)r];
        }
    }
}