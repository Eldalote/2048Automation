using System.Collections;
using System.Collections.Generic;


namespace SearchEngine.Scripts
{
    public static class MoveOptionsGenerator
    {


        public static List<PlayerMoveOption> PlayerMoveOptions(HexBoard startBoard, ulong startScore, int firstDirection)
        {
            List<PlayerMoveOption> moveOptions = new List<PlayerMoveOption>();

            // First start with the firstDirection
            HexBoard resultBoard;
            ulong resultScore;
            bool moveSuccess;
            MoveDirection direction = (MoveDirection)firstDirection;
            (resultBoard, resultScore, moveSuccess) = HexBoardActions.MoveAndMerge(startBoard, startScore, direction);
            // If something happened (moveSuccess) then this is a valid move option. Add it to the List.
            if (moveSuccess)
            {
                PlayerMoveOption moveOption = new PlayerMoveOption(direction, resultBoard, resultScore);
                moveOptions.Add(moveOption);
            }

            // Then loop through the 4 move directions (0x0000 through 0x3000). Skip the one already done.
            for (int i = 0; i < 4; i++)
            {
                if (i != (firstDirection >> 12))
                {
                    // Do a move in this direction, and record the results.                    
                    direction = (MoveDirection)(i << 12);
                    (resultBoard, resultScore, moveSuccess) = HexBoardActions.MoveAndMerge(startBoard, startScore, direction);
                    // If something happened (moveSuccess) then this is a valid move option. Add it to the List.
                    if (moveSuccess)
                    {
                        PlayerMoveOption moveOption = new PlayerMoveOption(direction, resultBoard, resultScore);
                        moveOptions.Add(moveOption);
                    }
                }
                

            }
            // Return the list with possible move options.
            return moveOptions;
        }

        public static List<RandomPlacementOption> RandomPlacementOptions(HexBoard startBoard, ulong startScore, int firstPlacement)
        {
            List<RandomPlacementOption> placementOptions = new List<RandomPlacementOption>();
            // First calculate the number of empty spaces.
            int emptySpaces = HexBoardActions.CalculateNumberOfEmptySpaces(startBoard);
            // First, do the firstPlacement. The placement is stored in the int as the last hexadecimal, the value the two next bits.
            int firstLocation = firstPlacement & 0xF;
            int firstValue = (firstPlacement & 0xF0) >> 4;
            HexBoard firstBoard = HexBoardActions.SpawnNewBlock(startBoard, firstLocation, firstValue);
            placementOptions.Add(new RandomPlacementOption(firstBoard, startScore, firstPlacement));
            // Then loop for every empty space, add a block to that space, and add the new options to the list.
            for (int i = 0; i < emptySpaces; i++)
            {
                // If this loop is the one from the firstlocation, only do the one that was not yet done.
                if (i == firstLocation)
                {
                    if (firstValue == 1)
                    {
                        HexBoard boardValueTwo = HexBoardActions.SpawnNewBlock(startBoard, i, 2);
                        placementOptions.Add(new RandomPlacementOption(boardValueTwo, startScore, firstPlacement));
                    }
                    else
                    {
                        HexBoard boardValueOne = HexBoardActions.SpawnNewBlock(startBoard, i, 2);
                        placementOptions.Add(new RandomPlacementOption(boardValueOne, startScore, firstPlacement));
                    }
                }
                else
                {
                    // Also store the placement information (location + value << 4)
                    HexBoard boardValueOne = HexBoardActions.SpawnNewBlock(startBoard, i, 1);
                    int placementOne = i + 0x10;
                    HexBoard boardValueTwo = HexBoardActions.SpawnNewBlock(startBoard, i, 2);
                    int placementTwo = i + 0x20;
                    placementOptions.Add(new RandomPlacementOption(boardValueOne, startScore, placementOne));
                    placementOptions.Add(new RandomPlacementOption(boardValueTwo, startScore, placementTwo));
                }
                       
            }

            // Return the list
            return placementOptions;

        }

    }

    public struct PlayerMoveOption
    {

        public MoveDirection Direction;
        public HexBoard BoardResult;
        public ulong ScoreResult;       
        public PlayerMoveOption(MoveDirection direction, HexBoard boardResult, ulong scoreResult)
        {
            Direction = direction;
            BoardResult = boardResult;
            ScoreResult = scoreResult;
        }
    }

    public struct RandomPlacementOption
    {
        public HexBoard BoardResult;
        public ulong Score;
        public int Placement;
        public RandomPlacementOption(HexBoard boardResult, ulong score, int placement)
        {
            BoardResult = boardResult;
            Score = score;
            Placement = placement;
        }
    }

}