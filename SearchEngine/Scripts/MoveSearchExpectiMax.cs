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
        /// Expectimax algorithm.
        /// Optional splitting searches into seperate threads. The level at which this is done can be fully chosen.
        /// 
        /// 
        /// 
        /// </summary>

        const int positiveInfinity = 100000000;
        const int negativeInfinity = -positiveInfinity;  
        

        private SearchSettings _settings;
        private bool _stopSearching;

        public MoveSearchExpectiMax()
        {
            _settings = new SearchSettings();
            _stopSearching = false;
        }

        public MoveSearchExpectiMax(SearchSettings settings)
        {
            _settings = settings;
            _stopSearching = false;
        }



        public (int, uint, MoveDirection) StartSearch(HexBoard board, SearchSettings searchSettings)
        {
            _settings = searchSettings;
            _stopSearching = false;            
            int finalEvaluation;
            MoveDirection bestDirection;
            uint totalNodesSearched;
            // First check the settings for sense.
            if (_settings.UseThreading == false)
            {
                _settings.ThreadSplitDepth = 0;
            }

            if (_settings.UseIterativeDeepening)
            {
                // First, set up a timer that will cancel the search after the set delay, have a cancellation token for the timer.
                CancellationTokenSource timeOutCancellation = new();
                Task.Delay(_settings.SearchTimeMillies, timeOutCancellation.Token).ContinueWith((t) => TimeOutSearch());
                // Once the timer is set, start the search.
                (finalEvaluation, totalNodesSearched, bestDirection) = IterativeDeepening(board);
                // It is possible that the search returned before the timer ran out (forced game over situation), so check if the timeout needs to be cancelled.
                timeOutCancellation?.Cancel();
            }
            else
            {
                (finalEvaluation, totalNodesSearched, bestDirection) = SearchMoveThreadSplit(_settings.ThreadSplitDepth, _settings.SearchDepth, board, 0, true);
            }

            return (finalEvaluation, totalNodesSearched, bestDirection);
        }


        private (int, uint, MoveDirection) IterativeDeepening(HexBoard hexBoard)
        {
            // With iterative Deepening, the goal is to start with a search of 1, then do a search of 2, then a search of 3, etc.
            // When using a MiniMax algorithm with alpha beta pruning, this can actually result in a overall faster search if move ordering is used.
            // With the Expectimax algorithm pruning is unfortunately not possible, so Iterative deepening will not result in a faster search.
            // However, when searching to a maximum time limit, it is the only realistic option, so that a valid search result is always available when the
            // time runs out.
            int evaluation = 0; 
            MoveDirection bestDirection = MoveDirection.None;
            uint nodesSearched = 0;
            // Set the maximum search depth. If searching to maximum time, set it to maximum iterative deepening, else set it to the normal searchdepth.
            int MaxSearchDepth = _settings.SearchToMaxTime ? _settings.MaxDepthIterativeDeepening : _settings.SearchDepth;
            // Do a for loop from 1 to maxSearchDepth
            for (int i = 1; i <= MaxSearchDepth; i++)
            {                
                int evaluationThisSearch;
                uint nodesThisSearch;
                MoveDirection bestDirectionThisSearch;
                // If i is greater than 2 and threading is enabled, set the threadsplit depth either to settings threadsplitdepth or i -2, whichever is smaller.
                // If not, set it to 0.
                int threadSplitDepthThisSearch = ((i > 2) && _settings.UseThreading ) ? int.Min(i - 2, _settings.ThreadSplitDepth) : 0;
                // Do a search to depth i. If threaddepth is 0, a normal search without threading is done by the SearchMoveThreadSplit.
                (evaluationThisSearch, nodesThisSearch, bestDirectionThisSearch) = SearchMoveThreadSplit(threadSplitDepthThisSearch, i, hexBoard, 0, true);
                // Check if StopSearch is flagged. If it is, the results of this search cannot be trusted, as it was cut of partway, just break the loop.
                if (_stopSearching) 
                {
                    Console.WriteLine($"Max depth search completed: {i - 1}");
                    break; 
                }
                // If stopsearching is not flagged, update the overall results with the results of the search just done. Then continue with the loop.
                evaluation = evaluationThisSearch;
                bestDirection = bestDirectionThisSearch;
                nodesSearched += nodesThisSearch;        
                // If the Evaluation is negative infinity, that means all the possible move directions return a forced game over. No deeper search is needed.
                if (evaluation == negativeInfinity) { break; }
            }
            // Once the loop is done, either by reaching the max depth or by time being called, return the evaluations from the last completed search.
            return (evaluation, nodesSearched, bestDirection);
        }

        public (int, uint, MoveDirection) SearchMoveThreadSplit(int threadSplitDepth, int searchDepth, HexBoard hexBoard, ulong score, bool playerToMove)
        {            
            if (_stopSearching)
            {
                return (0,0, MoveDirection.None);
            }
            // If threadSplitDepth is 0, no thread splitting is required, return a normal search.
            if (threadSplitDepth == 0)
            {
                return SearchMove(searchDepth, hexBoard, score, playerToMove);
            }

            // First of all, if searchDepth is 0, return the evaluation function, 1 node searched, and a none.
            // This should never happen if this function is called correctly, but it's here just to be sure.
            if (searchDepth == 0)
            {
                return (PositionEvaluator.EvaluatePosition(hexBoard, score), 1, MoveDirection.None);
            }
            // If > 0, Thread splitting is required. 
            // Patern is similar to normal search, so, different depending on playerToMove
            if (playerToMove)
            {                
                // Generate a list of possible moves the player can make
                List<MoveOption> moveOptions = MoveOptionsGenerator.GeneratePlayerMoveOptions(hexBoard, score);

                // If the number of options is 0, that means the game is over. This is considered bad, so return negativeInfinity.
                // This does count as a node evaluated.
                if (moveOptions.Count == 0)
                {                    
                    return (negativeInfinity, 1, MoveDirection.None);
                }

                // Start with a best evaluation of "negative infinity", the search will (hopefully) find better results.
                int bestEvaluation = negativeInfinity;

                // Then loop over each move option, store the results, so they can be compared at the end.
                // Instead of the array of ints that store the results in the normal search, the results are stored in a list of "SearchResults". 
                // The SearchResults are simple structs that hold the evaluation of the search, and the best direction. This is done because in a parallel loop, the order 
                // in which the searches are finished is unknown, therefore the order of the results in the list is unknown. To know what bestmove belongs to what evaluation
                // these need to be stored together.
                List<SearchResult> searchResults = new List<SearchResult>();
                // Also keep track of the number of nodes searched.
                uint nodesSearched = 0;
                // Now instead of a for loop like in the normal search, a parallel.ForEach loop is done.
                Parallel.ForEach(moveOptions, moveOption =>
                {
                    // If threadSplitDepth is greater than 1, more splitting is requested on the lower level, so call searchMoveThreadSplit again, else call normal search.
                    int evaluation;
                    uint nodesThisSearch;                   
                    
                    if (threadSplitDepth > 1)
                    {
                        (evaluation, nodesThisSearch, _) = SearchMoveThreadSplit(threadSplitDepth -1, searchDepth -1, moveOption.BoardResult, moveOption.ScoreResult, false);
                    }
                    else
                    {
                        (evaluation, nodesThisSearch, _) = SearchMove(searchDepth - 1, moveOption.BoardResult, moveOption.ScoreResult, false);
                    }
                    // Keep track of the number of nodes searched.
                    nodesSearched += nodesThisSearch;
                    // Add the results of this search to the searchResults list.
                    searchResults.Add(new SearchResult(evaluation, moveOption.Direction));
                });
                MoveDirection bestDirection = MoveDirection.None;
                // Once all the parallel searches are complete, compare the results.
                for (int i = 0; i < searchResults.Count; i++)
                {
                    // If the evaluation is better than or equal to the current best, set is as best.
                    // >= is used, because that way it is guaranteed that one of the valid moveoptions is selected, even if they all come back negative infinity.
                    if (searchResults[i].Evaluation >= bestEvaluation)
                    {
                        bestEvaluation = searchResults[i].Evaluation;
                        bestDirection = searchResults[i].BestDirection;
                    }
                }
                // The searches are done, return the results.
                return (bestEvaluation, nodesSearched, bestDirection);
            }
            else
            {
                // This is the random block placement. The evaluation for this search will be the average of all the possible random block placement evaluations.
                // Given that a chance of game over (negative infinity) has a very high weight due to the value being so large negative, the player evaluator isn't likely
                // to select an option with a possibility for a game over, unless all the options have a game over possibility.
                int averageEvaluation = 0;
                uint nodesSearched = 0;

                // Create a list of all the possible HexBoards variations from a random block placement.
                List<HexBoard> randomBlockOptions = MoveOptionsGenerator.GenerateRandomBlockOptions(hexBoard);

                // Get a search evaluation of each possibility. The parallel.ForEach loop is a lot simpler here, since no "best" needs to be found out. 
                // All is needed is a total (to be averaged), no order of results is important.
                Parallel.ForEach(randomBlockOptions, randomBlockOption =>
                {
                    uint nodesThisSearch;
                    int evaluationThisSearch;

                    // If threadSplitDepth is greater than 1, more splitting is requested at a lower level, so do the searches with threadsplitting, else use normal searches.
                    if (threadSplitDepth > 1)
                    {
                        (evaluationThisSearch, nodesThisSearch, _) = SearchMoveThreadSplit(threadSplitDepth - 1, searchDepth - 1, randomBlockOption, score, true);
                    }
                    else
                    {
                        (evaluationThisSearch, nodesThisSearch, _) = SearchMove(searchDepth - 1, randomBlockOption, score, true);
                    }
                    // Keep track of the number of nodes searched and the total for the average evaluation.
                    nodesSearched += nodesThisSearch;
                    averageEvaluation += evaluationThisSearch;
                });
                // Once all the Searches are done, devide by the number of options to get the average.
                averageEvaluation = averageEvaluation / randomBlockOptions.Count;
                // return the average, nodes searched, and a none.
                return (averageEvaluation, nodesSearched, MoveDirection.None);
            }
        }

        public (int, uint, MoveDirection) SearchMove(int searchDepth, HexBoard hexBoard, ulong score, bool playerToMove)
        {
            if (_stopSearching)
            {
                return (0, 0, MoveDirection.None);
            }
            // First of all, if searchDepth is 0, return the evaluation function, 1 node searched.
            if (searchDepth  == 0)
            {                
                return (PositionEvaluator.EvaluatePosition(hexBoard, score), 1, MoveDirection.None);
            }
            // If searchDepth is > 0, a deeper search is needed. How that search is done depends on playerToMove.
            if (playerToMove)
            {                
                // Generate a list of possible moves the player can make
                List<MoveOption> moveOptions = MoveOptionsGenerator.GeneratePlayerMoveOptions(hexBoard, score);

                // If the number of options is 0, that means the game is over. This is considered bad, so return negativeInfinity.
                // This does count as a node evaluated.
                if (moveOptions.Count == 0) 
                {                    
                    return (negativeInfinity, 1, MoveDirection.None);
                }

                // Start with a best evaluation of "negative infinity", the search will (hopefully) find better results.
                int bestEvaluation = negativeInfinity;
                uint nodesSearched = 0;

                // Then loop over each move option, store the results, so they can be compared at the end.
                int[] evaluationResults = new int[moveOptions.Count];
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    uint nodesThisSearch;
                    // Get the evaluation from this moveOption.
                    (evaluationResults[i], nodesThisSearch, _) = SearchMove(searchDepth - 1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false);
                    nodesSearched += nodesThisSearch;
                }
                // Compare the evaluation results, and select the best.
                // The comparisson is done here, so that all the searches have been done, and the _bestDirection can be set, without it being overwritten by
                // a search on a lower level.
                MoveDirection bestDirection = MoveDirection.None;
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    // If the evaluation is better than or equal to the current best, set is as best.
                    // >= is used, because that way it is guaranteed that one of the valid moveoptions is selected, even if they all come back negative infinity.
                    if (evaluationResults[i] >=  bestEvaluation)
                    {
                        bestEvaluation = evaluationResults[i];
                        bestDirection = moveOptions[i].Direction;
                    }
                }
                // The search is done, return the best evaluation found.
                return (bestEvaluation, nodesSearched, bestDirection);     
            }
            else
            {
                // This is the random block placement. The evaluation for this search will be the average of all the possible random block placement evaluations.
                // Given that a chance of game over (negative infinity) has a very high weight due to the value being so large negative, the player evaluator isn't likely
                // to select an option with a possibility for a game over, unless all the options have a game over possibility.
                int averageEvaluation = 0;
                uint nodesSearched = 0;

                // Create a list of all the possible HexBoards variations from a random block placement.
                List<HexBoard> randomBlockOptions = MoveOptionsGenerator.GenerateRandomBlockOptions(hexBoard);

                // Get a search evaluation of each possibility
                foreach (HexBoard randomOption in randomBlockOptions) 
                {
                    // Simply add all the evaluations together, devide later.
                    int evaluationThisSearch;
                    uint nodesThisSearch;
                    (evaluationThisSearch, nodesThisSearch, _) = SearchMove(searchDepth - 1, randomOption, score, true);
                    nodesSearched += nodesThisSearch;
                    averageEvaluation += evaluationThisSearch;
                }
                // Devide by the number of options.
                averageEvaluation = averageEvaluation / randomBlockOptions.Count;
                // return the average.
                return (averageEvaluation, nodesSearched, MoveDirection.None);
            }
        }      
        

        public void TimeOutSearch()
        {
            _stopSearching = true;            
        }

        private struct SearchResult
        {
            public int Evaluation;
            public MoveDirection BestDirection;
            public SearchResult()
            {
                Evaluation = 0;
                BestDirection = MoveDirection.None;
            }
            public SearchResult(int evaluation, MoveDirection bestDirection)
            { 
                Evaluation = evaluation; 
                BestDirection = bestDirection; 
            }
        }


    }
}
