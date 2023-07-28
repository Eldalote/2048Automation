using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;
using Debug = UnityEngine.Debug;
using System;

public class AutomatedMoveGenerator 
{
    private MoveSearcherOriginal _searcher;

    public AutomatedMoveGenerator() 
    {
        //_searcher = new AutomatedMoveSearcher();
    }


    public delegate void NextMove(MoveDirection direction);
    public static NextMove nextMove;
    

    public void GenerateNextMove(HexBoard hexBoard, ulong currentScore, bool threaded)
    {
        Task task = Task.Factory.StartNew(() => FindMove(hexBoard, currentScore, threaded));       
        Task.Delay(5000).ContinueWith((t) => StopMoveFinder());
        

    }

    private void FindMove(HexBoard hexBoard, ulong currentScore, bool threaded)
    {
        int depth = 6;
        MoveSearcherWorking searcherWorking = new MoveSearcherWorking();
        MoveSearcherOriginal searcherOriginal = new MoveSearcherOriginal();              
        Stopwatch watch = new Stopwatch();
        MoveDirection direction = MoveDirection.Left;
        watch.Start();
        if (threaded)
        {
            direction = searcherWorking.StartSearch(hexBoard, currentScore, depth, true);
            //direction = moveSearcherThreadedDoubleParalell.StartSearch(hexBoard, currentScore, depth);
        }
        else
        {
            direction = searcherOriginal.StartSearch(hexBoard, currentScore, depth);            
        }
        watch.Stop();
        TimeSpan timeSpan = watch.Elapsed;
        Debug.Log($"Search complete: Direction: {direction}, Time taken, seconds: {timeSpan.Seconds}, millis: {timeSpan.Milliseconds}");
        nextMove?.Invoke(direction);
    }

    private void StopMoveFinder()
    {

        
        
    }



}
