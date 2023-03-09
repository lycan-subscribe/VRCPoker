using System;
using UnityEngine;

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

                PrintArr(ranks);

                types[i] = GetType( ranks, suits );
            }

            return new int[] { 0 };
        }

        private static HAND_TYPE GetType(Rank[] ranks, Suit[] suits){
            // assert ranks.Length == suits.Length

            int numPairs;
            Rank[] pairs = new Rank[ ranks.Length / 2 ]; // Ordered, pairs[0] is highest pair (if TOAK exists, it's in here also)
            Rank[] three_of_a_kind = new Rank[ ranks.Length / 3 ]; // If FOAK exists, it's in here also
            Rank[] four_of_a_kind = new Rank[ ranks.Length / 4 ];
            int[] flush_indices = new int[5];
            bool royal_flush = false;
            int[] straight_indices = new int[5];
            Rank straight_rank = Rank.DNE; // Highest card in straight

            for(int i=0; i<ranks.Length; i++){
                // Scan for four of a kind
                for(int j=0; j<three_of_a_kind.Length; j++){
                    if(ranks[i] == three_of_a_kind[j])
                        InsertSorted(ref four_of_a_kind, ranks[i]);
                }

                // Scan for three of a kind
                for(int j=0; j<pairs.Length; j++){
                    if(ranks[i] == pairs[j])
                        InsertSorted(ref three_of_a_kind, ranks[i]);
                }

                // Scan for pairs
                for(int j=0; j<i; j++){
                    if( ranks[j] == ranks[i] )
                        InsertSorted(ref pairs, ranks[i]); // Duplicates will exist, but shouldn't matter
                }
            }

            PrintArr(pairs);
            PrintArr(three_of_a_kind);
            PrintArr(four_of_a_kind);

            return HAND_TYPE.HIGH_CARD;
        }



        // Why don't these exist lmao

        private static T[] Concat<T>(T[] arr1, T[] arr2){
            T[] arrNew = new T[arr1.Length + arr2.Length];
            Array.Copy(arr1, arrNew, arr1.Length);
            Array.Copy(arr2, 0, arrNew, arr1.Length, arr2.Length);
            return arrNew;
        }

        private static int[] MaxIndices<T>(T[] arr) where T : Enum{
            if(arr.Length < 1) return new int[0];

            T highest = arr[0];
            int num = 1;
            for(int i=1; i<arr.Length; i++){
                if( arr[i].CompareTo(highest) > 0 ){
                    highest = arr[i];
                    num = 1;
                }
                else if( arr[i].CompareTo(highest) == 0 ){
                    num ++;
                }
            }
            // assert: highest == Max(arr), and
            //  there are num elements of arr with value highest
            
            int[] ret = new int[num];
            int index = 0;
            for(int i=0; i<arr.Length; i++){
                if(arr[i].CompareTo(highest) == 0){
                    ret[index] = i;
                    index++;
                }
            }

            return ret;
        }

        private static T Max<T>(T[] arr) where T : Enum{
            if(arr.Length < 1) return default(T);

            T highest = arr[0];
            for(int i=1; i<arr.Length; i++){
                if( arr[i].CompareTo(highest) > 0 ){
                    highest = arr[i];
                }
            }
            return highest;
        }

        // Largest to smallest
        private static void InsertSorted<T>(ref T[] arr, T val) where T : Enum{
            // Split array
            int index = 0;
            while( index < arr.Length && val.CompareTo(arr[index]) <= 0 ) index++;
            // arr[index] >= val > arr[index+1]

            // Shift right side of array by 1
            T temp = val;
            for(int i=index; i<arr.Length; i++){
                if(temp.CompareTo(default(T)) == 0) break;

                T next = arr[i];
                arr[i] = temp;
                temp = next;
            }
        }

        private static void PrintArr<T>(T[] arr) where T : Enum{
            foreach(T i in arr){
                Debug.Log(i.ToString() + " ");
            }
            Debug.Log("\n");
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
        STRAIGHT_FLUSH, // If straight flushes tie, split pot
        ROYAL_FLUSH
    }
}