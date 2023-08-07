using System.Collections;
using System.Collections.Generic;

namespace SearchEngine.Scripts
{
    public static class PositionEvaluator
    {

        public static int EvaluatePosition(HexBoard board, ulong score)
        {
            int evaluation = (int)score / 4;
            evaluation += HexBoardActions.CalculateNumberOfEmptySpaces(board) * 2;



            return (int)score;
        }

    }
}