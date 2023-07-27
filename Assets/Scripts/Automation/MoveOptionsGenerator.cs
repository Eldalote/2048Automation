using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class MoveOptionsGenerator 
{
    

    public static List<PlayerMoveOption> PlayerMoveOptions(HexBoard startBoard, ulong startScore)
    {
        List<PlayerMoveOption> moveOptions = new List<PlayerMoveOption>();
        // Loop through the 4 move directions (0 through 3).
        for (int i = 0; i < 4; i++)
        {
            // Do a move in this direction, and record the results.
            HexBoard resultBoard = new HexBoard();
            ulong resultScore = 0;
            bool moveSuccess = false;
            MoveDirection direction = (MoveDirection)i;            
            (resultBoard, resultScore, moveSuccess) = HexBoardActions.MoveAndMerge(startBoard, startScore, direction);
            // If something happened (moveSuccess) then this is a valid move option. Add it to the List.
            if(moveSuccess)
            {
                PlayerMoveOption moveOption = new PlayerMoveOption
                {
                    BoardResult = resultBoard,
                    ScoreResult = resultScore,
                    Direction = direction
                };
                moveOptions.Add(moveOption);
            }
            
        }
        // Return the list with possible move options.
        return moveOptions;
    }

    public static List<RandomPlacementOption> RandomPlacementOptions(HexBoard startBoard, ulong startScore)
    {
        List<RandomPlacementOption> placementOptions = new List<RandomPlacementOption>();
        // First calculate the number of empty spaces.
        int emptySpaces = HexBoardActions.CalculateNumberOfEmptySpaces(startBoard);
        // Loop for every empty space, add a block to that space, and add the new options to the list.
        for (int i = 0;i < emptySpaces;i++)
        {
            HexBoard boardValueOne = new HexBoard();
            HexBoard boardValueTwo = new HexBoard();
            boardValueOne = HexBoardActions.SpawnNewBlock(startBoard, i, 1);
            boardValueTwo = HexBoardActions.SpawnNewBlock(startBoard, i, 2);
            placementOptions.Add(new RandomPlacementOption { BoardResult = boardValueOne, Score = startScore });
            placementOptions.Add(new RandomPlacementOption { BoardResult = boardValueTwo, Score = startScore });
        }        

        // Return the list
        return placementOptions;

    }

}

public class PlayerMoveOption
{

    public MoveDirection Direction;
    public HexBoard BoardResult;
    public ulong ScoreResult;
    public PlayerMoveOption() { }
}

public class RandomPlacementOption
{
    public HexBoard BoardResult;
    public ulong Score;

    public RandomPlacementOption() { }
}
