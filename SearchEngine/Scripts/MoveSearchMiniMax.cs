using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngine.Scripts
{
    public class MoveSearchMiniMax
    {
        /// <summary>
        /// Most up to date version of the move searcher, Minimax version. 
        /// Features:
        /// Mini-max recursive searching.
        /// Parallel multithreading, double deep. (The first max 4 options are split into threads, and each followup random placement too.)
        /// Optional double deep parallelisation, double deep is probably slower on lower core count systems.
        /// Threading fully optional.
        /// Alpha-Beta Pruning (basic)
        /// Iterative Deepening with basic move ordering. A list of best moves in the previous search is kept and used first, hopefully pruning more.
        /// </summary>


        // Set values for minimum and maximum evaluation. Named as Infinity because of theory naming.
        const int positiveInfinity = 100000000;
        const int negativeInfinity = -positiveInfinity;

        private bool _useIterativeDeepening = false;
        private bool _useThreading = true;
        private MoveDirection _bestDirectionNonThreaded = MoveDirection.None;


        // Simply starts the search, and gives the answer back, optionally with some debug logging.   
        public (MoveDirection, uint, int) StartSearch(HexBoard board, SearchSettings settings)
        {
            // Set the Iterative Deepening flag
            _useIterativeDeepening = settings.UseIterativeDeepening;
            int maxSearchDepth;
            // If Iterative deepening is used, the searchDepth needs to be at least 4, so check to make sure it is, correct if not.
            if (_useIterativeDeepening)
            {
                if (settings.SearchDepth < 4)
                {
                    maxSearchDepth = 4;
                }
                else
                {
                    maxSearchDepth = settings.SearchDepth;
                }
            }
            else
            {
                maxSearchDepth = settings.SearchDepth;
            }


            if (settings.UseThreading == false)
            {
                uint nodesSearched;
                int evaluation;
                _useThreading = false;
                (nodesSearched, evaluation) = SearchMoveIterativeLevel(maxSearchDepth, board, 0, true, negativeInfinity, positiveInfinity);
                return (_bestDirectionNonThreaded, nodesSearched, evaluation);
            }





            _useThreading = true;
            return SearchMovesTopLevel(settings.SearchDepth, board, 0, settings.ThreadedDoubleDepth, negativeInfinity, positiveInfinity);

        }

        // The main recursive search function, after the initial 2 top level searches.
        public (uint, int, List<int>) SearchMoves(int depth, HexBoard board, ulong score, bool playerToMove, List<int> moveOrderInputList, int alpha, int beta)
        {
            List<int> moveDirectionsList = new List<int>();
            List<int> moveOrderInputListCopy = new(moveOrderInputList);
            // If the list is empty, do nothing, if it is not empty, remove the first entry in the copy
            if (moveOrderInputListCopy.Count > 0)
            {
                moveOrderInputListCopy.RemoveAt(0);
            }            
            int bestDirectionOrPlacement;

            // If depth is zero, return 1 (number of positions evaluated in this function) and the evaluation of the current board.
            if (depth == 0)
            {
                return (1, PositionEvaluator.EvaluatePosition(board, score), moveDirectionsList);
            }
            // Keep track of how many nodes are searched in this iteration.
            uint nodesSearched = 0;

            int bestEvaluation;
            // There are two "players", the player, and random number determining the placement of the next block.
            // This is the player, or the maximizing player in mini-max
            if (playerToMove)
            {
                
                // Start of with the worst possible evaluation, look for beter ones.
                bestEvaluation = negativeInfinity;
                // Generate a list with all possible move options, starting with the one that was best last time.
                // If this is a new depth that is reached for this branch, the input list will be empty, so generate with 0.
                List<PlayerMoveOption> moveOptions;
                if (moveOrderInputList.Count == 0) 
                {
                    moveOptions = MoveOptionsGeneratorWithMoveOrder.GeneratePlayerMoveOptionsWithMoveOrder(board, score, 0);
                }
                else
                {
                    moveOptions = MoveOptionsGeneratorWithMoveOrder.GeneratePlayerMoveOptionsWithMoveOrder(board, score, moveOrderInputList[0]);
                }
               


                // If no more moves are possible, it's game over. Return worst possible evaluation.
                if (moveOptions.Count == 0)
                {
                    return (0, negativeInfinity, moveDirectionsList);
                }
                // start with baseline best move of the first option;
                bestDirectionOrPlacement = (int)moveOptions[0].Direction;
                // Loop over the possible move options.
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    int evaluation;
                    uint nodesThisSearch;
                    // Get the evaluation of the move, plus how many nodes were searched to get that evaluation.
                    (nodesThisSearch, evaluation, moveDirectionsList) = SearchMoves(depth - 1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false, moveOrderInputListCopy, alpha, beta);
                    nodesSearched += nodesThisSearch;
                    // If this evaluation is better than the best evaluation, store it, also keep track that this direction is best (so far).
                    if (evaluation > bestEvaluation)
                    {
                        bestEvaluation = evaluation;
                        bestDirectionOrPlacement = (int)moveOptions[i].Direction;
                        
                    }      
                    // If the current best evaluation is higher than beta, this branch is already "too good" to be chosen by the minimizing player, so stop looking.
                    if (bestEvaluation > beta)
                    {
                        break;
                    }
                    // If the bestEvaluation is higher than alpha, store it to alpha.
                    if ( bestEvaluation >  alpha)
                    {
                        alpha = bestEvaluation;
                    }
                }
            }
            // This is the "minimizing player" or simply all the options for random placement.
            else
            {
                // Start with baseline best placement of 0x10 (value 1 on the first available space).
                bestDirectionOrPlacement = 0x10;
                // The worst possible evaluation for the minimizing player is positiveInfinity. Find better options.
                bestEvaluation = positiveInfinity;
                // Generate a list with all possible boardstates after a random placement.
                // If the list is empty generate with 0x10
                List<RandomPlacementOption> moveOptions;
                if (moveOrderInputList.Count == 0)
                {
                    moveOptions = MoveOptionsGeneratorWithMoveOrder.RandomPlacementOptionsWithMoveOrder(board, score, 0x10);
                }
                else
                {
                    moveOptions = MoveOptionsGeneratorWithMoveOrder.RandomPlacementOptionsWithMoveOrder(board, score, moveOrderInputList[0]);
                }
                // Loop over the possibilities.
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    int evaluation;
                    uint nodesThisSearch;
                    // Get the evaluation of the current board, and how many nodes were evaluated for that board.
                    (nodesThisSearch, evaluation, moveDirectionsList) = SearchMoves(depth - 1, moveOptions[i].BoardResult, moveOptions[i].Score, true, moveOrderInputListCopy, alpha, beta);
                    nodesSearched += nodesThisSearch;
                    // If the evaluation is better (lower) than the best, store it, also keep track of the placement for this result.
                    if (evaluation < bestEvaluation)
                    {
                        bestEvaluation = evaluation;
                        bestDirectionOrPlacement = moveOptions[i].Placement;
                    }
                    // If the current bestEvaluation is lower than alpha, it means that this branch is already worse for the maximizing player than the previous branch, so stop looking.
                    if (bestEvaluation < alpha)
                    {
                        break;
                    }
                    // If the bestEvaluation is less than beta, store in beta.
                    if (bestEvaluation < beta)
                    {
                        beta = bestEvaluation;
                    }
                }
            }
            List<int> moveListForReturn  = new List<int>();
            moveListForReturn.Add(bestDirectionOrPlacement);
            moveListForReturn.AddRange(moveDirectionsList);
            // Return how many nodes were searched and the best evaluation result, plus the list of movements and placements that resulted in this evaluation.
            return (nodesSearched, bestEvaluation, moveListForReturn);
        }
        // The Top level of the recursive search. This one splits the possible searches into parallel threads. Returns the best direction, in addition to normal search results. 
        private (MoveDirection, uint, int) SearchMovesTopLevel(int depth, HexBoard board, ulong score, bool doubleDeepParallel, int alpha, int beta)
        {
            // Keep track of the best direction, and how many nodes were searched.
            uint nodesSearched = 0;
            MoveDirection bestDirection = MoveDirection.None;
            // Start with worst possible.
            int bestEvaluation = negativeInfinity;
            // Generate list of all possible moves.
            List<PlayerMoveOption> moveOptions = MoveOptionsGeneratorWithMoveOrder.GeneratePlayerMoveOptionsWithMoveOrder(board, score, 0);
            // Make list for results for each move. Since the searches are parallel, it cannot be relied upon that they are resolve in a predictable order.
            List<Results> resultList = new List<Results>();
            // Do a search for each possible move on a seperate parallel thread.
            // When double deep is enabled, call the second level function, if not, call the normal search function.
            if (doubleDeepParallel)
            {
                Parallel.ForEach(moveOptions, move =>
                {
                    int evaluation;
                    uint nodesThisSearch;
                    // As normal, get results, but don't compare just yet, store them in the list.
                    (nodesThisSearch, evaluation) = SearchMovesSecondLevel(depth - 1, move.BoardResult, move.ScoreResult, alpha, beta);
                    nodesSearched += nodesThisSearch;
                    resultList.Add(new Results(evaluation, move));
                });
            }
            else
            {
                Parallel.ForEach(moveOptions, move =>
                {
                    int evaluation;
                    uint nodesThisSearch;                    
                    // As normal, get results, but don't compare just yet, store them in the list.
                    (nodesThisSearch, evaluation) = SearchMoveIterativeLevel(depth - 1, move.BoardResult, move.ScoreResult, false, alpha, beta);
                    nodesSearched += nodesThisSearch;
                    resultList.Add(new Results(evaluation, move));
                });
            }

            // Once all the results are in, compare them, and get the direction of the best result.
            for (int i = 0; i < resultList.Count; i++)
            {
                if (resultList[i].Evaluation > bestEvaluation)
                {
                    bestEvaluation = resultList[i].Evaluation;
                    bestDirection = resultList[i].MoveOption.Direction;
                }


            }
            // If all possible moves returned with evaluation of negativeInfinity, just go with the first possible.
            if (bestEvaluation == negativeInfinity)
            {
                bestDirection = moveOptions[0].Direction;
            }
            // Return best direction, how many nodes were searched, and the best evaluation.
            return (bestDirection, nodesSearched, bestEvaluation);
        }

        private (uint, int) SearchMoveIterativeLevel(int targetDepth, HexBoard board, ulong score, bool playerToMove ,int alpha, int beta)
        {
            int evaluation = 0;
            if (_useIterativeDeepening == false)
            {
                // If iterative deepening is not to be used, return a normal searchmove.
                uint nodesSearched;
                (nodesSearched, evaluation, _) = SearchMoves(targetDepth, board, score, playerToMove, new List<int>() ,alpha, beta);
                return (nodesSearched, evaluation);
            }
            // If iterative deepening is to be used, we need to keep going deeper and deeper, keeping track of the best moves until then, for alpha beta pruning optimization.
            List<int> bestMovesList = new List<int>();
            uint totalNodesSearched = 0;
            uint nodesThisSearch;
            // Loop deepening searches. Starting of with 1 depth, up to and including target depth. The best moves list of each time is stored and fed to the next deepening,
            // Hopefully, this will still be a good move, and a lot can be pruned.
            for (int searchDepth = 1; searchDepth <= targetDepth; searchDepth++)
            {
                (nodesThisSearch, evaluation, bestMovesList) = SearchMoves(searchDepth, board, score, playerToMove, bestMovesList, alpha, beta);
                totalNodesSearched += nodesThisSearch;
            }
            if (_useThreading == false)
            {
                if (bestMovesList.Count > 0)
                {
                    _bestDirectionNonThreaded = (MoveDirection)bestMovesList[0];
                }
                else
                {
                    _bestDirectionNonThreaded = MoveDirection.None;
                }
            }
            // After all the searches, return the total nodes, and the last evaluation.
            return (totalNodesSearched, evaluation);
        }

        // Second level of the recurse search. This one also splits the searches into parallel threads. Fairly sure this is only faster in high core count systems.
        private (uint, int) SearchMovesSecondLevel(int depth, HexBoard board, ulong score, int alpha, int beta)
        {

            uint nodesSearched = 0;
            int bestEvaluation = positiveInfinity;
            // Get all moves.
            List<RandomPlacementOption> moveOptions = new List<RandomPlacementOption>();
            moveOptions = MoveOptionsGeneratorWithMoveOrder.RandomPlacementOptionsWithMoveOrder(board, score, 0x10);
            // Store results in a list.
            List<int> evaluationList = new List<int>();
            // Split all searches into parallel threads.
            Parallel.ForEach(moveOptions, move =>
            {
                int eval;
                uint nodesThisSearch;                
                (nodesThisSearch, eval) = SearchMoveIterativeLevel(depth - 1, move.BoardResult, move.Score, true, alpha, beta);
                //(nodesThisSearch, eval, _) = SearchMoves(depth - 1, move.BoardResult, move.Score, true, new List<int>(), alpha, beta);
                nodesSearched += nodesThisSearch;
                // Get results, store them in list for comparisson once all are in.
                evaluationList.Add(eval);
            });
            // Compare them.
            for (int i = 0; i < evaluationList.Count; i++)
            {
                if (evaluationList[i] < bestEvaluation)
                {
                    bestEvaluation = evaluationList[i];
                }
            }
            return (nodesSearched, bestEvaluation);
        }
        private struct Results
        {
            public int Evaluation;
            public PlayerMoveOption MoveOption;
            public Results(int evaluation, PlayerMoveOption moveOption)
            {
                Evaluation = evaluation;
                MoveOption = moveOption;
            }
        }
    }
}