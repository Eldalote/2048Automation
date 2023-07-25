using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AutomatedMoveSearcher 
{
    private HexBoard _board;
    private ulong _score;

    const int positiveInfinity = 100000;
    const int negativeInfinity = -positiveInfinity;

    int nodesSearched = 0;

    private MoveDirection _bestDirection = MoveDirection.None;
    
        
    private FastGameActions _moveMaker;

    public delegate void SearchComplete(MoveDirection direction);
    public static SearchComplete searchComplete;



    public AutomatedMoveSearcher(HexBoard hexBoard, ulong score) 
    {
        _board = hexBoard;
        _score = score;
    }

    public MoveDirection StartSearch(HexBoard board, ulong score)
    {
        int eval = SearchMoves(6, board, score, true, true);

        Debug.Log($"Total end states evaluated: {nodesSearched}, end eval: {eval}");
        return _bestDirection;
    }

    private int SearchMoves(int depth, HexBoard board, ulong score, bool playerToMove, bool topLevel)
    {
        if (depth == 0)
        {
            nodesSearched++;
            return PositionEvaluator.EvaluatePosition(board, score);
        }

        int bestEval = 0;
        if (playerToMove)
        {
            bestEval = negativeInfinity;
            List<PlayerMoveOption> moveOptions = new List<PlayerMoveOption>();
            moveOptions = MoveGenerator.PlayerMoveOptions(board, score);            

            for (int i = 0; i < moveOptions.Count; i++)
            {
                
                int eval = SearchMoves(depth -1, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false, false);
                if (eval > bestEval)
                {                   
                    bestEval = eval;
                    if (topLevel)
                    {
                        _bestDirection = moveOptions[i].Direction;
                    }
                    
                }
               
            }
            if (topLevel && (bestEval == negativeInfinity))
            {
                _bestDirection = moveOptions[0].Direction;
            }
        }
        else
        {
            bestEval = positiveInfinity;
            List<RandomPlacementOption> moveOptions = new List<RandomPlacementOption>();
            moveOptions = MoveGenerator.RandomPlacementOptions(board, score);
            if (moveOptions.Count == 0)
            {
                // Game over scenario
                return negativeInfinity;
            }

            for (int i = 0; i < moveOptions.Count; i++)
            {
                int eval = SearchMoves(depth -1, moveOptions[i].BoardResult, moveOptions[i].Score, true, false);
                if (eval < bestEval)
                {
                    bestEval = eval;
                }
            }            
        }

        return bestEval;
    }

    public void AbortSearch()
    {

    }
}
