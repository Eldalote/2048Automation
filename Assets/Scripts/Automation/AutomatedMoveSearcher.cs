using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AutomatedMoveSearcher 
{
    private HexBoard _board;
    private ulong _score;

    const int maxValue = 100000;
    const int minValue = -maxValue;

    int nodesSearched = 0;

    private MoveDirection _bestDirection = MoveDirection.None;
    private int _bestEval = minValue;
        
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
        SearchMoves(3, board, score, true);

        Debug.Log($"Nodes searched {nodesSearched}");
        return _bestDirection;
    }

    private int SearchMoves(int depth, HexBoard board, ulong score, bool playerToMove)
    {
        if (depth == 0)
        {
            nodesSearched++;
            return PositionEvaluator.EvaluatePosition(board, score);
        }
            

        if (playerToMove)
        {
            List<PlayerMoveOption> moveOptions = new List<PlayerMoveOption>();
            moveOptions = MoveGenerator.PlayerMoveOptions(board, score);            

            for (int i = 0; i < moveOptions.Count; i++)
            {
                //Debug.Log(i);
                int eval = SearchMoves(depth, moveOptions[i].BoardResult, moveOptions[i].ScoreResult, false);
                if (eval > _bestEval)
                {
                    Debug.Log($"Best eval {eval} direction {moveOptions[i].Direction}");
                    _bestDirection = moveOptions[i].Direction;
                    _bestEval = eval;
                }
            }
        }
        else
        {
            List<RandomPlacementOption> moveOptions = new List<RandomPlacementOption>();
            moveOptions = MoveGenerator.RandomPlacementOptions(board, score);

            for (int i = 0; i < moveOptions.Count; i++)
            {
                int eval = SearchMoves(depth -1, moveOptions[i].BoardResult, moveOptions[i].Score, true);
                if ( eval > _bestEval)
                {
                    _bestEval = eval;
                }
            }
        }

        return _bestEval;
    }

    public void AbortSearch()
    {

    }
}
