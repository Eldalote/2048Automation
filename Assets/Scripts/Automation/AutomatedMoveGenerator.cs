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
using System.IO;

public class AutomatedMoveGenerator 
{
    private MoveSearcherOriginal _searcher;
    private Process engineProcess;
    private Process writerProcess;
    private StreamWriter writerStream;

    public AutomatedMoveGenerator() 
    {
        //_searcher = new AutomatedMoveSearcher();
    }


    public delegate void NextMove(MoveDirection direction);
    public static NextMove nextMove;
    

    public void GenerateNextMove(HexBoard hexBoard, ulong currentScore, bool threaded)
    {
        String searcherPath = Application.dataPath;
        Debug.Log(searcherPath);
        searcherPath = searcherPath.Substring(0, searcherPath.LastIndexOf('/'));
        //searcherPath += "/SearchEngine/bin/Release/net7.0/SearchEngine.exe";
        searcherPath += "/SearchEngine/BuildAndRun.bat";
        Debug.Log(searcherPath);
        ProcessStartInfo startInfo = new ProcessStartInfo(searcherPath);
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardInput = true;

        String writerPath = Application.dataPath;
        writerPath = writerPath.Substring(0, writerPath.LastIndexOf('/'));        
        writerPath += "/ConsoleWriter/bin/Release/net7.0/ConsoleWriter.exe";
        //writerPath += "/ConsoleWriter/BuildAndRun.bat";
        ProcessStartInfo writerInfo = new ProcessStartInfo(writerPath);
        writerInfo.CreateNoWindow = false;
        writerInfo.UseShellExecute = false;
        writerInfo.RedirectStandardInput = true;
        writerInfo.RedirectStandardOutput = false;
        
        try
        {
            writerProcess = Process.Start(writerInfo);
           
        }
        catch (Exception ex)
        {
            Debug.Log($"Error starting writer: {ex}");
        }
        writerStream = writerProcess.StandardInput;
        writerStream.WriteLine("Writer Unity message");



        try
        {
            engineProcess = Process.Start(startInfo);            
        }
        catch (Exception e)
        {
            Debug.Log($"Error starting Searcher: {e}");
        }

        engineProcess.OutputDataReceived += ListenForMove;
        engineProcess.BeginOutputReadLine();
        





        
        //Task task = Task.Factory.StartNew(() => FindMove(hexBoard, currentScore, threaded));       
        //Task.Delay(5000).ContinueWith((t) => StopMoveFinder());
        

    }

    private void ListenForMove(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            Debug.Log(outLine.Data);
            writerStream.WriteLine(outLine.Data);
           
        }
    }

    private void FindMove(HexBoard hexBoard, ulong currentScore, bool threaded)
    {
        int depth = 5;
        MoveSearcherWorking searcherWorking = new MoveSearcherWorking();
        MoveSearcherOriginal searcherOriginal = new MoveSearcherOriginal();              
        Stopwatch watch = new Stopwatch();
        MoveDirection direction;
        uint nodesSearched;
        int evaluation;

        watch.Start();
        if (threaded)
        {
            (direction, nodesSearched, evaluation) = searcherWorking.StartSearch(hexBoard, currentScore, depth, true);
            //direction = moveSearcherThreadedDoubleParalell.StartSearch(hexBoard, currentScore, depth);
        }
        else
        {
            (direction, nodesSearched, evaluation) = searcherOriginal.StartSearch(hexBoard, currentScore, depth);            
        }
        watch.Stop();
        TimeSpan timeSpan = watch.Elapsed;
        Debug.Log($"Search complete: Direction: {direction}, nodes searched: {nodesSearched}, final evaluation: {evaluation}, Time taken, seconds: {timeSpan.Seconds}, millis: {timeSpan.Milliseconds}");
        nextMove?.Invoke(direction);
    }

    private void StopMoveFinder()
    {

        
        
    }



}
