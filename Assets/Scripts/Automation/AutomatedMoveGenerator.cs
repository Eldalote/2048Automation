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
    private AutomatedMoveSearcher _searcher;

    public AutomatedMoveGenerator() 
    {
        //_searcher = new AutomatedMoveSearcher();
    }


    public delegate void NextMove(MoveDirection direction);
    public static NextMove nextMove;
    

    public void GenerateNextMove(HexBoard hexBoard, ulong currentScore)
    {
        Task task = Task.Factory.StartNew(() => FindMove(hexBoard, currentScore));       
        Task.Delay(5000).ContinueWith((t) => StopMoveFinder());
        

    }

    private void FindMove(HexBoard hexBoard, ulong currentScore)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        AutomatedMoveSearcher searcher = new AutomatedMoveSearcher(hexBoard, currentScore);
        MoveDirection direction = searcher.StartSearch(hexBoard, currentScore);

        watch.Stop();
        TimeSpan timeSpan = watch.Elapsed;
        Debug.Log($"Search complete: Direction: {direction}, Time taken, seconds: {timeSpan.Seconds}, millis: {timeSpan.Milliseconds}");
        nextMove?.Invoke(direction);
    }

    private void StopMoveFinder()
    {

        
        
    }



}
