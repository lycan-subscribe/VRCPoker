using System;
using UnityEngine;

namespace VRCPoker {
    public static class WinningHandSolver {
        const bool ace_is_also_low = true;

        public static int[] GetWinningHands(CardHand commonCards, CardHand[] playerCards){
            
            if( playerCards.Length == 0 ){
                //Something very bad has happened
                return new int[0];
            }
            else if( playerCards.Length == 1 ){
                return new int[] { 0 };
            }


            HAND_TYPE highest_type = HAND_TYPE.NONE;
            Rank highest_straight = Rank.DNE;
            Rank highest_foak = Rank.DNE;
            Rank highest_toak = Rank.DNE;
            Rank highest_pair = Rank.DNE;
            Rank highest_spare_card = Rank.DNE;

            int[] winners = new int[] { };

            for(int player=0; player<playerCards.Length; player++){

                Rank[] ranks = Concat(commonCards.cardRanks, playerCards[player].cardRanks);
                Suit[] suits = Concat(commonCards.cardSuits, playerCards[player].cardSuits);
                // assert ranks.Length == suits.Length, ranks.Length >= 5


                // Get pairs, TOAK, FOAK

                Rank[] pairs = new Rank[ ranks.Length / 2 ]; // Ordered, pairs[0] is highest pair (if TOAK exists, it's in here also)
                Rank[] three_of_a_kind = new Rank[ ranks.Length / 3 ]; // If FOAK exists, it's in here also
                Rank[] four_of_a_kind = new Rank[ ranks.Length / 4 ];

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


                // Get high card

                Rank high_card = Rank.DNE;

                for(int i=0; i<ranks.Length; i++){
                    if( ranks[i] > high_card ) high_card = ranks[i];
                }


                // Get full house

                int full_house_pair_index = -1;

                if( three_of_a_kind[0] != Rank.DNE ){
                    for(int i=0; i<pairs.Length; i++){
                        if(pairs[i] == Rank.DNE) break;
                        else if(pairs[i] != three_of_a_kind[0]){
                            full_house_pair_index = i;
                            break;
                        }
                    }
                }


                // Check for straight + straight flush using a bitmask
                // https://stackoverflow.com/questions/32896845/check-for-poker-straight

                int rank_bitmask = 0;
                Rank straight_rank = Rank.DNE; // Highest card in straight
                //int[] straight_indices = new int[5];
                //bool straight_flush = false;

                for(int i=0; i<ranks.Length; i++){
                    rank_bitmask |= (1 << (int)ranks[i]);
                    if(ranks[i] == Rank.Ace) rank_bitmask |= 1;
                }

                for(int i=4; i<=13; i++){
                    if( (rank_bitmask & (0x1F << (i-4))) == (0x1F << (i-4)) ){
                        straight_rank = (Rank) i;
                    }
                }


                // Check for any flush

                int[] suit_counts = new int[] {0, 0, 0, 0, 0};
                bool flush = false;

                for(int i=0; i<suits.Length; i++){
                    suit_counts[ (int)suits[i] ] ++;
                }

                for(int i=1; i<suit_counts.Length; i++){
                    if( suit_counts[i] >= 5 ) flush = true;
                }


                // DEBUG

                PrintArr(ranks);
                PrintArr(pairs);
                PrintArr(three_of_a_kind);
                PrintArr(four_of_a_kind);
                Debug.Log("Highest straight: " + straight_rank + " (rank bitmask: " + rank_bitmask + ")");
                Debug.Log("Highest spare card: " + high_card);
                PrintArr(suits);
                PrintArr(suit_counts);


                // Figure out the type of hand

                HAND_TYPE current_type = HAND_TYPE.HIGH_CARD;

                if(false){ // royal flush

                }
                else if(false){ // straight flush

                }
                else if(four_of_a_kind[0] != Rank.DNE){
                    current_type = HAND_TYPE.FOUR_OF_A_KIND;
                }
                else if(full_house_pair_index >= 0){
                    current_type = HAND_TYPE.FULL_HOUSE;
                }
                else if(flush){
                    current_type = HAND_TYPE.FLUSH;
                }
                else if(straight_rank != Rank.DNE){
                    current_type = HAND_TYPE.STRAIGHT;
                }
                else if(three_of_a_kind[0] != Rank.DNE){
                    current_type = HAND_TYPE.THREE_OF_A_KIND;
                }
                else if(pairs[0] != Rank.DNE && pairs[1] != Rank.DNE){
                    current_type = HAND_TYPE.TWO_PAIR;
                }
                else if(pairs[0] != Rank.DNE){
                    current_type = HAND_TYPE.ONE_PAIR;
                }

                Debug.Log( current_type );
                Debug.Log("");
                Debug.Log("");
                Debug.Log("");


                // Compare with current winner

                if( current_type > highest_type ){
                    highest_type = current_type;
                    highest_straight = straight_rank;
                    highest_spare_card = high_card;
                    highest_pair = pairs[0];
                    highest_toak = three_of_a_kind[0];
                    highest_foak = four_of_a_kind[0];

                    winners = new int[]{ player };
                }
                else if( current_type == highest_type ){ // Uh oh a tie
                    // SO MANY EDGE CASES

                    if(current_type == HAND_TYPE.STRAIGHT || current_type == HAND_TYPE.STRAIGHT_FLUSH || current_type == HAND_TYPE.ROYAL_FLUSH){
                        if( straight_rank == highest_straight ){
                            winners = Concat( winners, new int[] { player } );
                        }
                        else if( straight_rank > highest_straight ){
                            highest_straight = straight_rank;
                            winners = new int[]{ player };
                        }
                    }
                    else if(current_type == HAND_TYPE.FLUSH){
                        winners = Concat( winners, new int[] { player } );
                    }
                    else if(current_type == HAND_TYPE.HIGH_CARD){
                        if( high_card == highest_spare_card ){
                            winners = Concat( winners, new int[] { player } );
                        }
                        else if( high_card > highest_spare_card ){
                            highest_spare_card = high_card;
                            winners = new int[]{ player };
                        }
                    }
                }
                
            }

            Debug.Log("WINNERS:");
            PrintArr(winners);

            return winners;
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

        /*
        // Smallest to largest, match_swaps gets shuffled the same as arr does
        private static void Sort<T,U>(ref T[] arr, ref U[] values) where T: Enum{
            Sort(ref arr, ref values, 0, arr.Length-1);
        }

        // High is inclusive
        private static void Sort<T,U>(ref T[] arr, ref U[] values, int low, int high) where T: Enum{
            if (low < high) {
    
                // pi is partitioning index, arr[p]
                // is now at right place
                int pi = Partition(ref arr, ref values, low, high);
    
                // Separately sort elements before
                // partition and after partition
                Sort(ref arr, ref values, low, pi - 1);
                Sort(ref arr, ref values, pi + 1, high);
            }
        }

        private static int Partition<T,U>(ref T[] arr, ref U[] values, int low, int high) where T: Enum
        {
    
            // pivot
            T pivot = arr[high];
    
            // Index of smaller element and
            // indicates the right position
            // of pivot found so far
            int i = (low - 1);
    
            for (int j = low; j <= high - 1; j++) {
    
                // If current element is smaller
                // than the pivot
                if (arr[j].CompareTo(pivot) < 0) {
    
                    // Increment index of
                    // smaller element
                    i++;
                    Swap(ref arr, ref values, i, j);
                }
            }
            Swap(ref arr, ref values, i + 1, high);
            return (i + 1);
        }

        private static void Swap<T,U>(ref T[] arr, ref U[] values, int i, int j) where T: Enum{
            T temp_1 = arr[i];
            U temp_2 = values[i];
            arr[i] = arr[j];
            values[i] = values[j];
            arr[j] = temp_1;
            values[j] = temp_2;
        }*/

        private static void PrintArr<T>(T[] arr){
            foreach(T i in arr){
                Debug.Log(i.ToString() + " ");
            }
            Debug.Log("\n");
        }
    }

    enum HAND_TYPE {
        NONE,
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