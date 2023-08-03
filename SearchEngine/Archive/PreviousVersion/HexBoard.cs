using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Archive.PreviousVersion
{
    internal class HexBoard
    {
        // Ulong containing the least significant bits of each space.
        public ulong LSB;
        // Ulong containing the most significant bits of each space.
        public ulong MSB;
    }




    internal static class HexBoardActions
    {
        // Split up the HexBoard into Rows, full value lines
        public static FullValueLines GetFullValueRows(HexBoard board)
        {
            // New working values.
            FullValueLines fullValueRows = new FullValueLines();

            // Loop through the rows, and extract the full values.
            for (int i = 0; i < 4; i++)
            {
                // The mask is 0xFFFF shifted over i rows.
                ulong selectionMask = ((ulong)0xFFFF << (16 * i));
                // Get LSB and MSB for the right row
                ulong LSB, MSB;
                LSB = board.LSB & selectionMask;
                MSB = board.MSB & selectionMask;
                // Shift them over to be at the end.
                LSB >>= (i * 16);
                MSB >>= (i * 16);
                // Combine them. abcd(lsb) + ABCD(msb) => AaBbCcDd
                ulong fullValueLong = (LSB & 0xF) | ((MSB & 0xF) << 4) | ((LSB & 0xF0) << 4) | ((MSB & 0xF0) << 8) |
                    ((LSB & 0xF00) << 8) | ((MSB & 0xF00) << 12) | ((LSB & 0xF000) << 12) | ((MSB & 0xF000) << 16);
                fullValueRows.Lines[i] = fullValueLong;
            }
            return fullValueRows;
        }
        // Split up the HexBoard into Columns, full value lines
        public static FullValueLines GetFullValueColumns(HexBoard board)
        {
            // New working variable.
            FullValueLines fullValueColumns = new FullValueLines();

            // Loop through the columns, and extract the full values.
            for (int i = 0; i < 4; i++)
            {
                // Mask is the columns, shifted over 1 hex per itteration.
                ulong selectionMask = ((ulong)0x000F000F000F000F << (4 * i));
                // Get LSB and MSB for the right row
                ulong LSB, MSB;
                LSB = board.LSB & selectionMask;
                MSB = board.MSB & selectionMask;
                // Shift them over to be at the end.
                LSB >>= (i * 4);
                MSB >>= (i * 4);
                // Combine them. 000a000b000c000d(lsb) + 000A000B000C000D(msb) => AaBbCcDd
                ulong fullValueLong = (LSB & 0xF) | ((MSB & 0xF) << 4) | ((LSB & 0xF0000) >> 8) | ((MSB & 0xF0000) >> 4) |
                   ((LSB & 0xF00000000) >> 16) | ((MSB & 0xF00000000) >> 12) | ((LSB & 0xF000000000000) >> 24) | ((MSB & 0xF000000000000) >> 20);
                fullValueColumns.Lines[i] = fullValueLong;
            }
            return fullValueColumns;
        }
        // Remake a HexBoard from full value lines, in Rows form.
        public static HexBoard RebuildHexBoardFromFullValueRows(FullValueLines fullValueRows)
        {
            // New working variable.
            HexBoard board = new HexBoard();

            // Loop through the lines, split them up, and put them in the right place in the hexBoard.
            for (int i = 0; i < 4; i++)
            {
                ulong LSB, MSB;
                // Split into the LSBs and MSBs.
                // AaBbCcDc => abcd + ABCD
                LSB = (fullValueRows.Lines[i] & 0xF) | ((fullValueRows.Lines[i] & 0xF00) >> 4) |
                    ((fullValueRows.Lines[i] & 0xF0000) >> 8) | ((fullValueRows.Lines[i] & 0xF000000) >> 12);

                MSB = ((fullValueRows.Lines[i] & 0xF0) >> 4) | ((fullValueRows.Lines[i] & 0xF000) >> 8) |
                    ((fullValueRows.Lines[i] & 0xF00000) >> 12) | ((fullValueRows.Lines[i] & 0xF0000000) >> 16);
                // Shift them to the left i rows
                LSB <<= (i * 16);
                MSB <<= (i * 16);
                // Add them to the hexBoard
                board.LSB += LSB;
                board.MSB += MSB;
            }
            return board;
        }
        // Remake a HexBoard from full value lines, in Columns form.
        public static HexBoard RebuildHexBoardFromFullValueColumns(FullValueLines fullValueColumns)
        {
            // New working variable.
            HexBoard board = new HexBoard();

            // Loop through the lines, split them up, and put them in the right place in the hexBoard.
            for (int i = 0; i < 4; i++)
            {
                ulong LSB, MSB;
                // Split into the LSBs and MSBs.
                // AaBbCcDc => 000a000b000c000d + 000A000B000C000D
                LSB = (fullValueColumns.Lines[i] & 0xF) | ((fullValueColumns.Lines[i] & 0xF00) << 8) |
                    ((fullValueColumns.Lines[i] & 0xF0000) << 16) | ((fullValueColumns.Lines[i] & 0xF000000) << 24);

                MSB = ((fullValueColumns.Lines[i] & 0xF0) >> 4) | ((fullValueColumns.Lines[i] & 0xF000) << 4) |
                    ((fullValueColumns.Lines[i] & 0xF00000) << 12) | ((fullValueColumns.Lines[i] & 0xF0000000) << 20);
                // Shift them to the left i columns
                LSB <<= (i * 4);
                MSB <<= (i * 4);
                // Add them to the hexBoard
                board.LSB += LSB;
                board.MSB += MSB;
            }
            return board;
        }

        // Helpfull for testing, prints the content of a hexboard to string, and optionally to debug.log.
        public static string PrintHexBoard(HexBoard board, bool print)
        {
            string hexBoardString = new string("");
            FullValueLines fullValueLines = new FullValueLines();
            fullValueLines = GetFullValueRows(board);
            // Start at 3 working down to 0, because the game display has row 3 on top.
            for (int y = 3; y >= 0; y--)
            {
                for (int x = 0; x < 4; x++)
                {
                    // Get the value of the space, and add the the to string to the string.
                    ulong spaceValue = (fullValueLines.Lines[y] >> (8 * x)) & 0xFF;
                    ulong printValue = 0;
                    if (spaceValue != 0)
                    {
                        printValue = (ulong)1 << (int)spaceValue;
                    }

                    hexBoardString = hexBoardString + printValue.ToString().PadLeft(6, '0') + ", ";
                }
                hexBoardString += "\n";
            }
            if (print)
            {
                //Debug.Log(hexBoardString);
            }
            return hexBoardString;
        }

        // Functions working with the HexBoard class. Only does the move and merge, does not place new block.
        // Returns Hexboard after move-merge, ulong score after move-merge, and bool whether move and/or merge happened.
        public static (HexBoard, ulong, bool) MoveAndMerge(HexBoard originalBoard, ulong OriginalScore, MoveDirection direction)
        {
            // Set up variables we can work with.
            ulong score = OriginalScore;
            HexBoard board = new HexBoard { LSB = originalBoard.LSB, MSB = originalBoard.MSB};
            FullValueLines lines = new FullValueLines();
            // If the direction is left or right, get rows.
            if (direction == MoveDirection.Left || direction == MoveDirection.Right)
            {
                lines = HexBoardActions.GetFullValueRows(board);
            }
            // if the direction is up or down, get columns.
            else
            {
                lines = HexBoardActions.GetFullValueColumns(board);
            }
            // The lines are now ready for moving down or left, but need to be flipped for moving up or right.
            if (direction == MoveDirection.Up || direction == MoveDirection.Right)
            {
                lines.FlipLines();
            }

            // After this, execute move and merge on the lines, note how much the score increased.
            ulong scoreincrease = lines.MoveMergeLines();
            score += scoreincrease;

            // After the move merge, the hexboard needs to be rebuild from the lines.
            // First, if the lines were flipped before, now flip them back.
            if (direction == MoveDirection.Up || direction == MoveDirection.Right)
            {
                lines.FlipLines();
            }
            // Then rebuild from columns or rows depending on direction.
            // If the direction is left or right, rebuild form rows.
            if (direction == MoveDirection.Left || direction == MoveDirection.Right)
            {
                board = HexBoardActions.RebuildHexBoardFromFullValueRows(lines);
            }
            // if the direction is up or down, rebuild from columns.
            else
            {
                board = HexBoardActions.RebuildHexBoardFromFullValueColumns(lines);
            }
            // Check if the board is still the same, and note the result.
            bool changeHappened = false;
            if (board.LSB != originalBoard.LSB || board.MSB != originalBoard.MSB)
            {
                changeHappened = true;
            }
            // Return the results
            return (board, score, changeHappened);

        }
        // Function to calculate the number of empty spaces on the board.
        public static int CalculateNumberOfEmptySpaces(HexBoard board)
        {
            int emptySpaces = 0;
            // Loop over every space.
            for (int i = 0; i < 16; i++)
            {
                // Check if the value of the space is 0, if it is, increment emptySpaces by one
                if (((board.LSB & ((ulong)0xF << (i * 4))) == 0) && ((board.MSB & ((ulong)0xF << (i * 4))) == 0))
                {
                    emptySpaces++;
                }

            }
            return emptySpaces;
        }

        public static HexBoard SpawnNewBlock(HexBoard board, int locationRandom, int valueRandom)
        {
            int countDown = locationRandom;
            HexBoard workingBoard = new HexBoard { LSB = board.LSB, MSB = board.MSB };
            // Loop over every space.
            for (int i = 0; i < 16; i++)
            {
                // Check if the value of the space is 0, if it is, check countDown. If countdown is 0, place new block here, if not, countDown--
                if ((board.LSB & ((uint)0xF << (i * 4))) == 0 && (board.MSB & ((uint)0xF << (i * 4))) == 0)
                {
                    if (countDown == 0)
                    {
                        workingBoard.LSB += (ulong)valueRandom << (i * 4);
                    }
                    else
                    {
                        countDown--;
                    }
                }

            }

            return workingBoard;
        }



    }
}
