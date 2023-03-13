using System;
using UnityEngine;

namespace VRCPoker {
    public static class WinningHandSolver {
        const bool ace_is_also_low = true;

        public static int[] GetWinningHands(CardHand commonCards, CardHand[] playerCards){
            string winMessage = "";
            return GetWinningHands(commonCards, playerCards, ref winMessage);
        }

        public static int[] GetWinningHands(CardHand commonCards, CardHand[] playerCards, ref string winMessage){
            winMessage = "";
            
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


                // Check for straight + straight flush using an array
                // https://stackoverflow.com/questions/32896845/check-for-poker-straight

                Rank straight_rank = Rank.DNE; // Highest card in straight
                int[][] rank_indices = new int[14][]; // e.g. rank_indices[ (int) Rank.Two ] would be an array of indices i where ranks[i] == Rank.Two
                for(int i=0; i<rank_indices.Length; i++) rank_indices[i] = new int[]{};
                bool straight_flush = false;

                for(int i=0; i<ranks.Length; i++){
                    rank_indices[ (int)ranks[i] ] = Concat(rank_indices[i], new int[]{ i });

                    if(ranks[i] == Rank.Ace){
                        rank_indices[0] = Concat(rank_indices[i], new int[]{ i });
                    }
                }

                for(int i=4; i<rank_indices.Length; i++){
                    if( rank_indices[i-4].Length > 0 && rank_indices[i-3].Length > 0 && rank_indices[i-2].Length > 0 && rank_indices[i-1].Length > 0 && rank_indices[i].Length > 0 ){
                        // Test for flush
                        bool[] flush_of_suit = new bool[] {false, true, true, true, true}; // 5 long because Suit.DNE is 0
                        bool found_flush = false;
                        for(int j=i-4; j<=i; j++){
                            bool[] suit_of_rank = new bool[] {false, false, false, false, false};
                            foreach( int rank_index in rank_indices[j] ){
                                suit_of_rank[ (int)suits[rank_index] ] = true;
                            }
                            for(int k=1; k<suit_of_rank.Length; k++){
                                flush_of_suit[k] = flush_of_suit[k] && suit_of_rank[k];
                            }
                        }
                        // At this point, e.g. flush_of_suit[ (int)Suit.Spades ] is true iff there is one spade for each rank in the straight
                        foreach(bool suit_flush in flush_of_suit){
                            if( suit_flush ) found_flush = true;
                        }
                        
                        if(found_flush){
                            straight_rank = (Rank) i;
                            straight_flush = true;
                        }
                        else if(!straight_flush){ // Don't replace the highest rank if the previous one was a flush and this one isn't
                            straight_rank = (Rank) i;
                        }

                    }
                }


                // Check for any flush
                
                int[] suit_counts = new int[] {0, 0, 0, 0, 0}; // 5 long because Suit.DNE is 0
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
                Debug.Log("Highest straight: " + straight_rank);
                Debug.Log("Found straight flush: " + straight_flush);
                Debug.Log("Highest spare card: " + high_card);
                PrintArr(suits);
                PrintArr(suit_counts);


                // Figure out the type of hand

                HAND_TYPE current_type = HAND_TYPE.HIGH_CARD;

                if(straight_flush && straight_rank == Rank.Ace){ // royal flush
                    current_type = HAND_TYPE.ROYAL_FLUSH;
                }
                else if(straight_flush){ // straight flush
                    current_type = HAND_TYPE.STRAIGHT_FLUSH;
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
                    winMessage = "won with " + current_type.GetName();
                }
                else if( current_type == highest_type ){ // Uh oh a tie
                    // SO MANY EDGE CASES

                    if(current_type == HAND_TYPE.STRAIGHT || current_type == HAND_TYPE.STRAIGHT_FLUSH || current_type == HAND_TYPE.ROYAL_FLUSH){
                        if( straight_rank == highest_straight ){
                            winners = Concat( winners, new int[] { player } );
                            winMessage = "tied with " + current_type.GetName() + " high " + straight_rank.GetName();
                        }
                        else if( straight_rank > highest_straight ){
                            highest_straight = straight_rank;
                            winners = new int[]{ player };
                            winMessage = "won with " + current_type.GetName() + " high " + straight_rank.GetName();
                        }
                    }
                    else if(current_type == HAND_TYPE.FLUSH){
                        winners = Concat( winners, new int[] { player } );
                        winMessage = "tied with " + current_type.GetName();
                    }
                    else if(current_type == HAND_TYPE.HIGH_CARD){
                        if( high_card == highest_spare_card ){
                            winners = Concat( winners, new int[] { player } );
                            winMessage = "tied with a high " + high_card.GetName();
                        }
                        else if( high_card > highest_spare_card ){
                            highest_spare_card = high_card;
                            winners = new int[]{ player };
                            winMessage = "won with a high " + high_card.GetName();
                        }
                    }
                    else if(current_type == HAND_TYPE.ONE_PAIR || current_type == HAND_TYPE.TWO_PAIR){
                        if( pairs[0] == highest_pair ){
                            winners = Concat( winners, new int[] { player } );
                            winMessage = "tied with " + current_type.GetName() + " high " + pairs[0].GetName();
                        }
                        else if( pairs[0] > highest_pair ){
                            highest_pair = pairs[0];
                            winners = new int[]{ player };
                            winMessage = "won with " + current_type.GetName() + " high " + pairs[0].GetName();
                        }
                    }
                    else if(current_type == HAND_TYPE.THREE_OF_A_KIND || current_type == HAND_TYPE.FULL_HOUSE){
                        if( three_of_a_kind[0] == highest_toak ){
                            winners = Concat( winners, new int[] { player } );
                            winMessage = "tied with " + current_type.GetName() + " high " + three_of_a_kind[0].GetName();
                        }
                        else if( three_of_a_kind[0] > highest_toak ){
                            highest_toak = three_of_a_kind[0];
                            winners = new int[]{ player };
                            winMessage = "won with " + current_type.GetName() + " high " + three_of_a_kind[0].GetName();
                        }
                    }
                    else if(current_type == HAND_TYPE.FOUR_OF_A_KIND){
                        if( four_of_a_kind[0] == highest_foak ){
                            winners = Concat( winners, new int[] { player } );
                            winMessage = "tied with " + current_type.GetName() + " high " + four_of_a_kind[0].GetName();
                        }
                        else if( four_of_a_kind[0] > highest_foak ){
                            highest_foak = four_of_a_kind[0];
                            winners = new int[]{ player };
                            winMessage = "won with " + current_type.GetName() + " high " + four_of_a_kind[0];
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

    static class HAND_TYPEMethods{
        public static string GetName(this HAND_TYPE t){
            string[] hand_names = { // .ToString() completely breaks during udon runtime, no idea why
                "nothing",
                "a high card",
                "a pair",
                "two pairs",
                "three of a kind",
                "a straight",
                "a flush",
                "a full house",
                "a four of a kind",
                "a straight flush",
                "a royal flush"
            };

            return hand_names[(int)t];
        }
    }
}