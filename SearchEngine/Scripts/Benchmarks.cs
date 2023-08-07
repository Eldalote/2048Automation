using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace SearchEngine.Scripts
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [RankColumn]
    public class Benchmarks
    {
        private HexBoard HexBoardToTest = new(35254718980866, 0);

        private static readonly MoveSearchExpectiMax moveSearcher = new();       

        public void ManualBenchmark(int depth)
        {
            HexBoard followup = new(1301880187751829811, 0);
            HexBoard hexBoard = new(1301880187751829811, 0);
            string hexstring = HexBoardActions.PrintHexBoard(followup, false);
            Console.WriteLine(hexstring);
            SearchSettings settings = new SearchSettings();
            settings.SearchDepth = depth;
            MoveSearchExpectiMax searcher = new(settings);
            Stopwatch watch = new();
            watch.Start();
            (int eval, uint nodes, MoveDirection direction) = searcher.StartSearch(hexBoard, settings);
            watch.Stop();
            TimeSpan timeSpan = watch.Elapsed;            
            Console.WriteLine($"Final evaluation: {eval}, nodes evaluated: {nodes}, best direction: {direction}. Took time: {timeSpan}");
        }

        [Benchmark(Baseline = true)]
        public void NoThreading()
        {
            SearchSettings settings = new SearchSettings();
            settings.UseThreading = false;
            moveSearcher.StartSearch(HexBoardToTest, settings);
        }
        [Benchmark]
        public void ThreadingOne()
        {
            SearchSettings settings = new SearchSettings();
            settings.UseThreading = true;
            settings.ThreadSplitDepth = 1;
            moveSearcher.StartSearch(HexBoardToTest, settings);
        }

        [Benchmark]
        public void ThreadingTwo()
        {
            SearchSettings settings = new SearchSettings();
            settings.UseThreading = true;
            settings.ThreadSplitDepth = 2;
            moveSearcher.StartSearch(HexBoardToTest, settings);
        }

        [Benchmark]
        public void ThreadingThree()
        {
            SearchSettings settings = new SearchSettings();
            settings.UseThreading = true;
            settings.ThreadSplitDepth = 3;
            moveSearcher.StartSearch(HexBoardToTest, settings);
        }

    }
}