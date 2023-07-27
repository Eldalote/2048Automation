using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class AutomatedMoveSearcher 
{
    

    const int positiveInfinity = 100000;
    const int negativeInfinity = -positiveInfinity;

    private uint _nodesSearched = 0;    
    private MoveDirection _bestDirection = MoveDirection.None;   
    

    public delegate void SearchComplete(int evaluation, MoveDirection direction);
    public static SearchComplete searchComplete;



    public AutomatedMoveSearcher() 
    {
        
    }

    public MoveDirection StartSearch(HexBoard board, ulong score, bool threaded)
    {
        
        if (threaded)
        {
            int endEval = SortOutThreaded(7, board, score);
            Debug.Log($"Total end states evaluated: {_nodesSearched}, end eval: {endEval}");
            return _bestDirection;
        }
        else
        {
            int eval = SearchMovesTopLevel(7, board, score);

            Debug.Log($"Total end states evaluated: {_nodesSearched}, end eval: {eval}");
            return _bestDirection;
        }
        
    }

    public int SearchMoves(int depth, HexBoard board, ulong score, bool playerToMove)
    {
        if (depth == 0)
        {
            _nodesSearched++;
            return PositionEvaluator.EvaluatePosition(board, score);
        }

        int bestEval;
        if (playerToMove)
        {
            bestEval = negativeInfinity;
            List<PlayerMoveOption> moveOptions = new List<PlayerMoveOption>();
            moveOptions = MoveOptionsGenerator.PlayerMoveOptions(board, score);     
            
            // If no more moves are possible, it's game over.
            if (moveOptions.Count == 0)
            {
                return negativeInfinity;
            }

            for (int i = 0; i < moveOptions.Count; i++)
            {
                
                int eval = SearchMoves(depth -1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false);
                if (eval > bestEval)
                {                   
                    bestEval = eval;                                    
                }               
            }            
        }
        else
        {
            bestEval = positiveInfinity;
            List<RandomPlacementOption> moveOptions = new List<RandomPlacementOption>();
            moveOptions = MoveOptionsGenerator.RandomPlacementOptions(board, score);            

            for (int i = 0; i < moveOptions.Count; i++)
            {
                int eval = SearchMoves(depth -1, moveOptions[i].BoardResult, moveOptions[i].Score, true);
                if (eval < bestEval)
                {
                    bestEval = eval;
                }
            }            
        }

        return bestEval;
    }

    private int SearchMovesTopLevel(int depth, HexBoard board, ulong score)
    {
        int bestEval;
        
        bestEval = negativeInfinity;
        List<PlayerMoveOption> moveOptions = MoveOptionsGenerator.PlayerMoveOptions(board, score);

        for (int i = 0; i < moveOptions.Count; i++)
        {

            int eval = SearchMoves(depth - 1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false);
            if (eval > bestEval)
            {
                bestEval = eval;
                _bestDirection = moveOptions[i].Direction;                    
            }
        }
        if (bestEval == negativeInfinity)
        {
            _bestDirection = moveOptions[0].Direction;
        }
        

        return bestEval;
    }
    private async Task<int[]> SearchMovesTopLevelAsync(int depth, HexBoard board, ulong score)
    {
        
        List<PlayerMoveOption> moveOptions = MoveOptionsGenerator.PlayerMoveOptions(board, score);
        int[] evaluationArray = new int[moveOptions.Count];
        List<Task<int>> taskList = new List<Task<int>>();

        for (int i = 0; i < moveOptions.Count; i++)
        {
            //AutomatedMoveSearcher newSearcher = new AutomatedMoveSearcher();
            //taskList.Add(Task.Run(() => newSearcher.SearchMoves(depth - 1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false)));
            taskList.Add(Task.Run(() => SearchMoves(depth - 1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false)));
            await taskList[i];
            
        }        
        return await Task.WhenAll(taskList);
    }
    private int SortOutThreaded(int depth, HexBoard board, ulong score)
    {
        int bestEval = negativeInfinity;

        Task<int[]> evals = SearchMovesTopLevelAsync(depth, board, score);
        
        List<PlayerMoveOption> moveOptions = MoveOptionsGenerator.PlayerMoveOptions(board, score);

        for (int i = 0; i < moveOptions.Count; i++)
        {
            
            if (evals.Result[i] > bestEval)
            {
               
                bestEval = evals.Result[i];
                _bestDirection = moveOptions[i].Direction;
            }
        }
        
        if (bestEval == negativeInfinity)
        {
            _bestDirection = moveOptions[0].Direction;
        }        
        return bestEval;

    }
    
    public void AbortSearch()
    {

    }
}
