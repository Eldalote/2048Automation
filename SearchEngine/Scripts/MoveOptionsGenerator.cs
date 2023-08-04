using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Scripts
{
    internal static class MoveOptionsGenerator
    {

        public static List<MoveOption> GeneratePlayerMoveOptions(HexBoard startBoard, ulong startScore)
        {
            List<MoveOption> moveOptions = new List<MoveOption>();
            // Loop through the 4 move directions (0 through 3).
            for (int i = 0; i < 4; i++)
            {
                // Do a move in this direction, and record the results.
                HexBoard resultBoard = new HexBoard();
                ulong resultScore = 0;
                bool moveSuccess = false;
                MoveDirection direction = (MoveDirection)(i << 12);
                (resultBoard, resultScore, moveSuccess) = HexBoardActions.MoveAndMerge(startBoard, startScore, direction);
                // If something happened (moveSuccess) then this is a valid move option. Add it to the List.
                if (moveSuccess)
                {
                    MoveOption moveOption = new MoveOption(direction, resultBoard, resultScore);                    
                    moveOptions.Add(moveOption);
                }

            }
            // Return the list with possible move options.
            return moveOptions;
        }

        public static List<HexBoard> GenerateRandomBlockOptions(HexBoard startBoard)
        {
            List<HexBoard> placementOptions = new List<HexBoard>();
            // First calculate the number of empty spaces.
            int emptySpaces = HexBoardActions.CalculateNumberOfEmptySpaces(startBoard);
            // Loop for every empty space, add a block to that space, and add the new options to the list.
            for (int i = 0; i < emptySpaces; i++)
            {                            
                placementOptions.Add(HexBoardActions.SpawnNewBlock(startBoard, i, 1));
                placementOptions.Add(HexBoardActions.SpawnNewBlock(startBoard, i, 2));
            }

            // Return the list
            return placementOptions;

        }
    }

    internal struct MoveOption
    {

        public MoveDirection Direction;
        public HexBoard BoardResult;
        public ulong ScoreResult;
        public MoveOption()
        {
            Direction = MoveDirection.None;
            BoardResult = new HexBoard();
            ScoreResult = 0;
        }
        public MoveOption(MoveDirection direction, HexBoard boardResult, ulong scoreResult)
        {
            Direction = direction;
            BoardResult = boardResult;
            ScoreResult = scoreResult;
        }
    }    
}
