using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBoard
{
    // Ulong containing the least significant bits of each space.
    public ulong LSB;
    // Ulong containing the most significant bits of each space.
    public ulong MSB;
}

public static class HexBoardActions
{
    public static ulong[] GetFullValueRows(HexBoard board)
    {
        ulong[] fullValueRows = new ulong[4];





        return fullValueRows;
    }

    public static ulong[] GetFullValueColumns(HexBoard board)
    {
        ulong[] fullValueColumns = new ulong[4];




        return fullValueColumns;
    }

    public static HexBoard RebuildHexBoardFromFullValueRows(ulong[] fullValueRows)
    {
        HexBoard board = new HexBoard();

        return board;
    }
    
    public static HexBoard RebuildHexBoardFromFullValueColumns(ulong[] fullValueColumns)
    {
        HexBoard board = new HexBoard();


        return board;
    }
}
