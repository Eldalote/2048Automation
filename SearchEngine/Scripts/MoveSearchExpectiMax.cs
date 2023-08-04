using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Scripts
{
    public class MoveSearchExpectiMax
    {
        /// <summary>
        /// Most up to date version of the Expectimax version of the move searcher.
        /// Features:
        /// 
        /// 
        /// 
        /// 
        /// </summary>

        const int positiveInfinity = 100000000;
        const int negativeInfinity = -positiveInfinity;

        private uint _nodesSearched = 0;
        private MoveDirection _bestDirection = MoveDirection.None;



        public int SearchMoveThreadSplit(int threadSplitDepth, int searchDepth, HexBoard hexBoard, ulong score, bool playerToMove)
        {
            // If threadSplitDepth is 0, no more thread splitting is required, return a normal search.
            if (threadSplitDepth == 0)
            {
                return SearchMove(searchDepth, hexBoard, score, playerToMove);
            }
            // If > 0, Thread splitting is required. 
            // Patern is similar to normal search, so, different depending on playerToMove
            if (playerToMove)
            {

            }
            else
            {

            }





            return 0;
        }

        public int SearchMove(int searchDepth, HexBoard hexboard, ulong score, bool playerToMove)
        {
            // First of all, if searchDepth is 0, return the evaluation function, and increment the number of nodes searched.
            if (searchDepth  == 0)
            {
                _nodesSearched++;
                return PositionEvaluator.EvaluatePosition(hexboard, score);
            }
            // If searchDepth is > 0, a deeper search is needed. How that search is done depends on playerToMove.
            if (playerToMove)
            {
                // Start with a best evaluation of "negative infinity", the search will (hopefully) find better results.
                int bestEvaluation = negativeInfinity;

                // Generate a list of possible moves the player can make
                List<MoveOption> moveOptions = MoveOptionsGenerator.GeneratePlayerMoveOptions(hexboard, score);

                // If the number of options is 0, that means the game is over. This is considered bad, so return negativeInfinity.
                // This does count as a node evaluated.
                if (moveOptions.Count == 0) 
                {
                    _nodesSearched++;
                    return negativeInfinity;
                }

                // Then loop over each move option, store the results, so they can be compared at the end.
                int[] evaluationResults = new int[moveOptions.Count];
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    // Get the evaluation from this moveOption.
                    evaluationResults[i] = SearchMove(searchDepth - 1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false);
                }
                // Compare the evaluation results, and select the best.
                // The comparisson is done here, so that all the searches have been done, and the _bestDirection can be set, without it being overwritten by
                // a search on a lower level.
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    // If the evaluation is better than or equal to the current best, set is as best.
                    // >= is used, because that way it is guaranteed that one of the valid moveoptions is selected, even if they all come back negative infinity.
                    if (evaluationResults[i] >=  bestEvaluation)
                    {
                        bestEvaluation = evaluationResults[i];
                        _bestDirection = moveOptions[i].Direction;
                    }
                }
                // The search is done, return the best evaluation found.
                return bestEvaluation;     
            }
            else
            {
                // This is the random block placement. The evaluation for this search will be the average of all the possible random block placement evaluations.
                // Given that a chance of game over (negative infinity) has a very high weight due to the value being so large negative, the player evaluator isn't likely
                // to select an option with a possibility for a game over, unless all the options have a game over possibility.
                int averageEvaluation = 0;

                // Create a list of all the possible HexBoards variations from a random block placement.
                List<HexBoard> randomBlockOptions = MoveOptionsGenerator.GenerateRandomBlockOptions(hexboard);

                // Get a search evaluation of each possibility
                foreach (HexBoard randomOption in randomBlockOptions) 
                {
                    // Simply add all the evaluations together, devide later.
                    averageEvaluation += SearchMove(searchDepth - 1, randomOption, score, true);
                }
                // Devide by the number of options.
                averageEvaluation = averageEvaluation / randomBlockOptions.Count;
                // return the average.
                return averageEvaluation;
            }
        }
        



    }
}
