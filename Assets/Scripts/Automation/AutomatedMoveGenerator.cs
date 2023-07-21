using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

public class AutomatedMoveGenerator 
{
    public AutomatedMoveGenerator() { }
    public delegate void NextMove(MoveDirection direction);
    public static NextMove nextMove;

    public void GenerateNextMove(ulong[] hexBoard, ulong currentScore)
    {
        Task task = Task.Factory.StartNew(() => FindMove(hexBoard, currentScore));        
        

    }

    private void FindMove(ulong[] hexBoard, ulong currentScore)
    {
        
        Thread.Sleep(1);
        
        Random rnd = new Random();
        int rand = rnd.Next(0, 4);         
        
        MoveDirection direction = MoveDirection.Left;
        
        if (rand == 0)
            direction = MoveDirection.Left;
        if (rand == 1)
            direction = MoveDirection.Right;
        if (rand == 2)
            direction = MoveDirection.Up;
        if (rand == 3)
            direction = MoveDirection.Down;
       
        nextMove?.Invoke(direction);
    }



}
