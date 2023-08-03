using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Archive.PreviousVersion
{
    internal class FullValueLines
    {
        // Ulong Array holding the 4 full value lines.
        public ulong[] Lines;
        // Constructor defining the length of the array.
        public FullValueLines()
        {
            Lines = new ulong[4];
        }

        // Flip each line from AaBbCcDd to DdCcBbAa.
        public void FlipLines()
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = ((Lines[i] & 0xFF) << 24) | ((Lines[i] & 0xFF00) << 8) | ((Lines[i] & 0xFF0000) >> 8) | ((Lines[i] & 0xFF000000) >> 24);
            }
        }
        // Move and merge on each line. Returns the increase in score caused by the merges.
        public ulong MoveMergeLines()
        {
            // Keep a scoreIncrease variable.
            ulong scoreIncrease = 0;

            // Loop through the lines.
            for (int i = 0; i < Lines.Length; i++)
            {
                // Only do work flipping the line if it is not 0.
                if (Lines[i] != 0)
                {
                    // If the first place is 0, shift over. Keep doing this until it is no longer 0, keep track of how many times this has been done,
                    // to shorten the later loops. While loop should never get stuck, since it just checked if the line is 0.
                    int emptyShifts = 0;
                    while ((Lines[i] & 0xFF) == 0)
                    {
                        Lines[i] >>= 8;
                        emptyShifts++;
                    }
                    // After this we check each space it is possible to move and/ or merge to.
                    // This is space 0, 1, and 2, since we won't move to space 3 (wrong direction). This can be shortened by the number of times
                    // already shifted over in the previous loop.
                    // Loop itterator mt (move target).
                    for (int mt = 0; mt < (3 - emptyShifts); mt++) // TESTING 3 -  is 4 - in old file.
                    {
                        // Create the masks for the space to move to, and the space to check.
                        ulong moveTargetMask = (ulong)0xFF << (mt * 8);
                        ulong checkSpaceMask = (ulong)0xFF00 << (mt * 8);
                        // Store the offset between moveTargetMask and checkLocationMask. Must be stored outside the inner loops, as it can be independantly increased in 
                        // different loops.
                        int moveCheckOffset = 8;

                        // If the check space is 0, find a block to move to this location.
                        if ((Lines[i] & moveTargetMask) == 0)
                        {
                            // Loop over the possible check spaces and find a non-0 value. The further along in the mt-loop the fewer spaces have to be checked.
                            // Loop itterator cs (CheckSpace)
                            for (int cs = 0; cs < (3 - (mt + emptyShifts)); cs++) // TESTING 3 or 4?
                            {
                                // If a non-0 value is found, move it to the move target space.
                                if ((Lines[i] & checkSpaceMask) != 0)
                                {
                                    // First set the found value to the move target space.
                                    // The difference in location is stored in moveCheckOffset.
                                    Lines[i] = Lines[i] | ((Lines[i] & checkSpaceMask) >> moveCheckOffset);
                                    // Then remove the value from it's original location.
                                    Lines[i] &= ~checkSpaceMask;
                                    // Since a block was found, the search must stop.
                                    break;
                                }
                                // Move the checkspace over one space.
                                checkSpaceMask <<= 8;
                                moveCheckOffset += 8;
                            }
                        }
                        // Now look again if the the value of the space is non-0 (a block might have just moved here), then check if a merge is possible.
                        if ((Lines[i] & moveTargetMask) != 0)
                        {
                            // The same loop as for the move is repeated, except now the value is also checked to match to the value in the moveTarget space.
                            // Loop itterator cs (CheckSpace)
                            for (int cs = 0; cs < (3 - (mt + emptyShifts)); cs++) // TESTING 3 or 4?
                            {
                                // If the checking space is 0, shift the check over, and continue the search.
                                if ((Lines[i] & checkSpaceMask) == 0)
                                {
                                    checkSpaceMask <<= 8;
                                    moveCheckOffset += 8;
                                }
                                // else if the checking space value is equal to the move target value, merge them, then break the search.
                                else if (((Lines[i] & checkSpaceMask) >> moveCheckOffset) == (Lines[i] & moveTargetMask))
                                {
                                    // Clear the value from the check space.
                                    Lines[i] &= ~checkSpaceMask;
                                    // While overflow of the new location is possible in the programming sense, it is not possible to actually get such a 
                                    // value in game, so it is not needed to check for overflow.
                                    // But the value is needed for the score calculation, so it needs to be extracted from the space.
                                    ulong spaceValue = (Lines[i] & moveTargetMask) >> (mt * 8);
                                    // Increase that value by one, and the space value by one.
                                    Lines[i] += ((ulong)1 << (mt * 8));
                                    spaceValue++;
                                    // Increase the score.
                                    scoreIncrease += ((ulong)1 << (int)spaceValue);
                                    // Search for merge target is over.
                                    break;

                                }
                                // If a value is found that is non-0, but also doesn't match the move target value, the search is over.
                                else { break; }

                            }
                        }
                    }
                }
            }
            // When that is all done, the score increase is returned.
            return scoreIncrease;
        }
    }
}
