using System;

namespace VRCPoker {
    public static class WinningHandSolver {
        public static int[] GetWinningHands(CardHand commonCards, CardHand[] playerCards){
            
            if( playerCards.Length == 0 ){
                //Something very bad has happened
                return new int[0];
            }
            else if( playerCards.Length == 1 ){
                return new int[] { 0 };
            }


            HAND_TYPE[] types = new HAND_TYPE[playerCards.Length];
            Rank[] highest_cards = new Rank[playerCards.Length];

            for(int i=0; i<playerCards.Length; i++){

                Rank[] ranks = Concat(commonCards.cardRanks, playerCards[i].cardRanks);
                Suit[] suits = Concat(commonCards.cardSuits, playerCards[i].cardSuits);

                types[i] = GetType( ranks, suits );

                highest_cards[i] = Max(playerCards[i].cardRanks);
            }

            return new int[] { 0 };
        }

        private static HAND_TYPE GetType(Rank[] ranks, Suit[] suits){
            return HAND_TYPE.HIGH_CARD;
        }

        // Why doesn't this exist lmao
        private static T[] Concat<T>(T[] arr1, T[] arr2){
            T[] arrNew = new T[arr1.Length + arr2.Length];
            Array.Copy(arr1, arrNew, arr1.Length);
            Array.Copy(arr2, 0, arrNew, arr1.Length, arr2.Length);
            return arrNew;
        }

        private static int MaxIndex<T>(T[] arr) where T : Enum{
            int highest = 0;
            for(int i=1; i<arr.Length; i++){
                if( arr[i].CompareTo(arr[highest]) > 0 ){
                    highest = i;
                }
            }
            return highest;
        }

        private static T Max<T>(T[] arr) where T : Enum{
            return arr[MaxIndex(arr)];
        }
    }

    enum HAND_TYPE {
        HIGH_CARD, // If high cards tie, use second highest card, etc
        ONE_PAIR, // If pairs tie, use highest card, then second, etc
        TWO_PAIR, // If highest pair ties, use second highest pair, then highest card
        THREE_OF_A_KIND, // If TOAK ties, use highest card, then second
        STRAIGHT, // If straight ties, split pot
        FLUSH, // If flushes tie, split pot
        FULL_HOUSE, // If TOAK ties, use pair
        FOUR_OF_A_KIND, // If FOAK ties, use highest card
        STRAIGHT_FLUSH // If straight flushes tie, split pot
    }

    /*class CardArray{
        Rank[] Ranks;
        Suit[] Suits;
        int Length;

        public CardArray(int size){
            Ranks = new Rank[size];
            Suits = new Suit[size];
            Length = 0;
        }

        public CardArray InsertCards(CardHand hand){
            for(int i=0; i<hand.Length; i++){
                Ranks[Length] = hand.cardRanks[i];
                Suits[Length] = hand.cardSuits[i];
                Length++;
            }

            return this;
        }
    }*/
}