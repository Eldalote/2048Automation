using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FastGameActionsPrototypeTwo
{
    // Constructor
    public FastGameActionsPrototypeTwo()
    {

    }

    // Main function, the move and merge.
    public ulong[] MoveMerge(ulong[] originalHexBoardArray, int newBlockRandomLocation, int newBlockValue, MoveDirection direction)
    {
        // Create the working variables named as lines. Upper and lower (values).
        ulong[] lineUpperArray = new ulong[4] { 0, 0, 0, 0 };
        ulong[] lineLowerArray = new ulong[4] { 0, 0, 0, 0 };

        // If the direction is left or right, split into rows.
        if (direction == MoveDirection.Left || direction == MoveDirection.Right)
        {
            // Define row masks.           
            ulong[] rowMaskArray = new ulong[4] {   0x000000000000FFFF,
                                                    0x00000000FFFF0000,
                                                    0x0000FFFF00000000,
                                                    0xFFFF000000000000};
            // If the upper hexboard is 0, we can just leave the upper lines at 0, otherwise we have to do work.
            if (originalHexBoardArray[1] != 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    lineUpperArray[i] = (originalHexBoardArray[1] & rowMaskArray[i]) >> (i * 16);
                }
            }
            // Now do the lower lines.
            for (int i = 0; i < 4; i++)
            {
                lineLowerArray[i] = (originalHexBoardArray[0] & rowMaskArray[i]) >> (i * 16);
            }
        }
        // If up or down, split into columns.
        else
        {
            // Define column masks.            
            ulong[] columnMaskArray = new ulong[4] {    0x000F000F000F000F,
                                                        0x00F000F000F000F0,
                                                        0x0F000F000F000F00,
                                                        0xF000F000F000F000};
            // If the upper hexboard is 0, we can just leave the upper lines at 0, otherwise we have to do work.
            if (originalHexBoardArray[1] != 0)
            {
                // They are all shifted over so that the first actual number is on the least significant spot, all equal.
                // Feed that into the BunchUpColumn function, so that they are all at the LSB side of the number.
                for (int i = 0; i < 4; i++)
                {
                    lineUpperArray[i] = BunchUpColumn((originalHexBoardArray[1] & columnMaskArray[i]) >> (i * 4));
                }
            }
            // Now do the same, but for the lower lines.            
            for (int i = 0; i < 4; i++)
            {
                lineLowerArray[i] = BunchUpColumn((originalHexBoardArray[0] & columnMaskArray[i]) >> (i * 4));
            }
        }
        // At this point, the lines are in the correct order for moving left or down,
        // but need to be reversed for moving up or right
        if (direction == MoveDirection.Up || direction == MoveDirection.Right)
        {
            // Only flip the lines if they are not 0.
            for (int i = 0; i < 4; i++)
            {
                if (lineUpperArray[i] != 0)
                {
                    lineUpperArray[i] = FlipLine(lineUpperArray[i]);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (lineLowerArray[i] != 0)
                {
                    lineLowerArray[i] = FlipLine(lineLowerArray[i]);
                }
            }
        }
        // Then combine each line pair into a single line holding the complete value.
        ulong[] linesCombinedValueArray = new ulong[4];
        for (int i = 0; i < 4; i++)
        {
            linesCombinedValueArray[i] = CombineLowerUpperIntoOne(lineLowerArray[i], lineUpperArray[i]);
        }
        // Then execute a move-merge on each line.
        for (int i = 0; i < 4; i++)
        {
            linesCombinedValueArray[i] = MoveMergeLine(linesCombinedValueArray[i]);
        }
        // Split the combined values back into the seperate parts.
        for (int i = 0; i < 4; i++)
        {
            ulong[] split = SplitCombinedValueIntoLowerUpper(linesCombinedValueArray[i]);
            lineLowerArray[i] = split[0];
            lineUpperArray[i] = split[1];
        }
        // For up or right moves, the lines need to be flipped back.
        if (direction == MoveDirection.Up || direction == MoveDirection.Right)
        {
            for (int i = 0; i < 4; i++)
            {
                // Only spend effort flipping if it isn't 0.
                if (lineLowerArray[i] != 0)
                {
                    lineLowerArray[i] = FlipLine(lineLowerArray[i]);
                }
                if (lineUpperArray[i] != 0)
                {
                    lineUpperArray[i] = FlipLine(lineUpperArray[i]);
                }
            }
        }
        // Combine the lines back into full boards.
        ulong[] newHexBoardArray = new ulong[2] { 0, 0 };
        if (direction == MoveDirection.Left || direction == MoveDirection.Right)
        {
            // For left to right it's simple. Add the lines to the hexgrid, shifted over depending on the line.
            for (int i = 0; i < 4; i++)
            {
                newHexBoardArray[0] += (lineLowerArray[i] << (i * 16));
                newHexBoardArray[1] += (lineUpperArray[i] << (i * 18));
            }
        }
        else
        {
            // For up down moves it's a bit harder, so we call a function to do it. 
            for (int i = 0; i < 4; i++)
            {
                newHexBoardArray[0] += SpreadOutColumn(lineLowerArray[i]) << (i * 4);
                newHexBoardArray[1] += SpreadOutColumn(lineUpperArray[i]) << (i * 4);
            }
        }
        // Now we have the complete hexboards after the move-merge, now we just need to spawn a new block if a move-merge happened.
        // We compare the new hexboard to the original hexboard, if it is unchanged, no new block.
        if (originalHexBoardArray != newHexBoardArray)
        {
            // Shift over newNumblockLocation blank spaces to spawn the new block.
            ulong shiftMask = 0xF;
            int shiftCount = 0;
            while (true)
            {
                // If the value of the space currently being checked is not 0, move over, without decreasing random count.
                if (((newHexBoardArray[0] & shiftMask) != 0) || ((newHexBoardArray[1] & shiftMask) != 0))
                {
                    shiftMask <<= 4;
                    shiftCount += 4;
                }
                // If it is 0, place the block if random = 0 (and break the loop!), else decrease random by one and move over a space.
                else
                {
                    if (newBlockRandomLocation == 0)
                    {
                        newHexBoardArray[0] += Convert.ToUInt64(newBlockValue) << shiftCount;
                        break;
                    }
                    else
                    {
                        shiftMask <<= 4;
                        shiftCount += 4;
                        newBlockRandomLocation--;
                    }
                }

            }
        }
        // This should be it.      
        return newHexBoardArray;
    }
    // Function to flip the hex values (of the last 4 places) of the given input
    private ulong FlipLine(ulong inputLine)
    {
        ulong flipped = new ulong();
        flipped = ((inputLine & 0xF) << 12) | ((inputLine & 0xF0) << 4) | ((inputLine & 0xF00) >> 4) | ((inputLine & 0xF000) >> 12);
        return flipped;
    }
    // Function to bunch up spread out column values.
    private ulong BunchUpColumn(ulong spreadColumn)
    {
        ulong bunched = new ulong();
        bunched = (spreadColumn & 0xF) | ((spreadColumn >> 12) & 0xF0)
                                | ((spreadColumn >> 24) & 0xF00) | ((spreadColumn >> 36) & 0xF000);
        return bunched;
    }
    // Function to spread out the column that was earlier bunched up.
    private ulong SpreadOutColumn(ulong bunchedUpColumn)
    {
        ulong spread = new ulong();
        spread = (bunchedUpColumn & 0xF) | (bunchedUpColumn & 0xF0) << 12 |
                            (bunchedUpColumn & 0xF00) << 24 | (bunchedUpColumn & 0xF000) << 36;
        return spread;
    }
    // Function to move and merge a single line. Takes an array holding the lower [0] and upper [0] values, returns same array.
    private ulong MoveMergeLine(ulong inputLine)
    {
        ulong lineToMoveMerge = new ulong();
        lineToMoveMerge = inputLine;
        // If the line is 0, return 0, don't do work.
        if (lineToMoveMerge == 0) { return 0; }
        // TODO: First move over if rightmost space is empty (and keep track of how many times done, to shorten the following loop)
        int x = 0;
        // Then go over each space and check for move or merge options.
        ulong moveTargetMask;
        ulong checkLocationMask;
        // We look at the first, second and third location to move to. We won't ever move to the fourth location.
        // Reduced by the times already shifted over.
        for (int i = 0; i < 4 - x; i++)
        {
            // Set the moveTargetMask and checkLocationMask
            moveTargetMask = (ulong)0xFF << (i * 8);
            checkLocationMask = (ulong)0xFF00 << (i * 8);
            // Store the offset between moveTargetMask and checkLocationMask. Must be stored outside the loop, as it can be independantly increased in 
            // different loops.
            int locationValueOffset = 8;
            // If the location we're currently looking at is 0, find a block to move here.
            if ((lineToMoveMerge & moveTargetMask) == 0)
            {
                // Loop over the possible checkLocations and find a non-0 value. The further we are in the i loop,
                // the less far we have to look. 
                for (int j = 0; j < 4 - (x + i); j++)
                {
                    if ((lineToMoveMerge & checkLocationMask) != 0)
                    {
                        // Set the found value to the moveTargetLocation
                        // The difference in location between moveTargetLocation and checkLocation is locationValueOffset.
                        lineToMoveMerge = lineToMoveMerge | ((lineToMoveMerge & checkLocationMask) >> locationValueOffset);
                        // Then remove the value from the checkLocation.
                        lineToMoveMerge &= ~checkLocationMask;
                        // Since we have found a block, we can stop the rest of the loop, search over.
                        break;
                    }
                    // If the current checkLocation is 0, move it up.
                    checkLocationMask <<= 8;
                    locationValueOffset += 8;
                }
            }
            // Now look again if there is a block (it might just have moved here) and check if there is a merge possebility.            
            if ((lineToMoveMerge & moveTargetMask) != 0)
            {
                for (int j = 0; j < 4 - (x + i); j++)
                {
                    // If the checking location is 0, move the checking location and continue with the loop.                    
                    if (((lineToMoveMerge & checkLocationMask) == 0))
                    {
                        checkLocationMask <<= 8;
                        locationValueOffset += 8;
                    }
                    // Else if the checking location is equal to the moveTargetLocation, merge, then break the loop.
                    else if (((lineToMoveMerge & checkLocationMask) >> locationValueOffset) == (lineToMoveMerge & moveTargetMask))
                    {
                        // Clear the value in the checkLocation.
                        lineToMoveMerge &= ~checkLocationMask;
                        // Increase the value in the target location by one. If the value is not already 0xFF
                        if ((lineToMoveMerge & moveTargetMask) == moveTargetMask)
                        {
                            Debug.Log("Wow, value overflow. Congratulations.");
                            return 0;
                        }
                        lineToMoveMerge += ((ulong)1 << (i * 8));
                        break;
                    }
                    // Else, there is a block that doesn't match, so no merge is possible, break the loop.
                    else
                    {
                        break;
                    }
                }
            }
        }
        // Should be done now, return the line.
        return lineToMoveMerge;

    }
    // Function that combines a line of an array of lower and upper values into a single number holding the complete value.
    private ulong CombineLowerUpperIntoOne(ulong lowerValue, ulong upperValue)
    {
        ulong combined = new ulong();
        combined = (lowerValue & 0xF) | ((upperValue & 0xF) << 4) |
                ((lowerValue & 0xF0) << 4) | ((upperValue & 0xF0) << 8) |
                ((lowerValue & 0xF00) << 8) | ((upperValue & 0xF00) << 12) |
                ((lowerValue & 0xF000) << 12) | ((upperValue & 0xF000) << 16);
        return combined;
    }
    // Function that splits the combined line into array of lower and upper values.
    private ulong[] SplitCombinedValueIntoLowerUpper(ulong combinedValue)
    {
        ulong[] split = new ulong[2];
        split[0] = (combinedValue & 0xF) | ((combinedValue & 0xF00) >> 4)
            | ((combinedValue & 0xF0000) >> 8) | ((combinedValue & 0xF000000) >> 12);
        split[1] = ((combinedValue & 0xF0) >> 4) | ((combinedValue & 0xF000) >> 8) |
            ((combinedValue & 0xF00000) >> 12) | ((combinedValue & 0xF0000000) >> 16);
        return split;
    }
    // Function for TESTING, print the hexboards to debug or returned as string.
    public string PrintHexBoard(ulong[] hexBoardArray)
    {
        string hexBoardString = new string("");
        for (int y = 3; y >= 0; y--)
        {
            for (int x = 0; x < 4; x++)
            {
                hexBoardString = hexBoardString + HexToString(hexBoardArray, new Vector2Int(x, y)) + ", ";
            }
            hexBoardString += "\n";
        }
        Debug.Log(hexBoardString);
        return hexBoardString;

    }
    // Function for TESTING, convert single hex space to string.
    private string HexToString(ulong[] hexBoardArray, Vector2Int hexLocation)
    {
        // Create new ulong array
        ulong[] printBoardArray = new ulong[2] { 0, 0 };
        // The new array values are the passed array shifted over by the grid location.
        printBoardArray[0] = hexBoardArray[0] >> (hexLocation.x * 4) + (hexLocation.y * 16);
        printBoardArray[1] = hexBoardArray[1] >> (hexLocation.x * 4) + (hexLocation.y * 16);
        // The value is added from lower and upper parts.
        ulong value = (printBoardArray[0] & 0xF) | ((printBoardArray[1] & 0xF) << 4);
        // For display, the game value is wanted, so do 2^value.
        int toThePower = Convert.ToInt32(value);
        value = (ulong)2 << (toThePower - 1);
        if (value == 1)
        {
            value = 0;
        }
        // Return string version of the game value.
        return value.ToString().PadLeft(6, '0');
    }
}
