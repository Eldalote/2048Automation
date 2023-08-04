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
        private HexBoard hexBoardToTest = new HexBoard { LSB = 0x100000, MSB = 0 };
        private Archive.Original.HexBoard hexBoardToTestOriginal = new Archive.Original.HexBoard { LSB = 0x100000, MSB = 0 };
        private Archive.PreviousVersion.HexBoard hexBoardToTestPrevious = new Archive.PreviousVersion.HexBoard { LSB = 0x100000, MSB = 0 };
        private ulong scoreToTest = 0;
        private int depthToTest = 6;

        private static readonly MoveSearchMiniMax moveSearcherWorking = new MoveSearchMiniMax();
        private static readonly Archive.Original.MoveSearcherOriginal moveSearcherOriginal = new Archive.Original.MoveSearcherOriginal();
        private static readonly Archive.Original.MoveSearcherOriginalThreaded moveSearcherOriginalThreaded = new Archive.Original.MoveSearcherOriginalThreaded();
        private static readonly Archive.PreviousVersion.MoveSearcherPreviousVersion moveSearcherPreviousVersion = new Archive.PreviousVersion.MoveSearcherPreviousVersion();

        private SearchSettings searchSettingsCurrent = new SearchSettings { UseIterativeDeepening = true, MaxSearchDepth = 6, ThreadedDoubleDepth = false, UseThreading = true };

        public void InUnityBenchmark(int depth)
        {         

            HexBoard[] boardToTestArray = new HexBoard[] {
            new HexBoard { LSB = 0x1001, MSB = 0 },
            new HexBoard { LSB = 0x102081, MSB = 0 },
            new HexBoard { LSB = 0x101101001, MSB = 0 },
            new HexBoard { LSB = 0x11110001001, MSB = 0 },
            new HexBoard { LSB = 0x12861001, MSB = 0 },
            new HexBoard { LSB = 0x01991221001, MSB = 0x1 },
            new HexBoard { LSB = 0x2002, MSB = 0 },
            new HexBoard { LSB = 0x3003, MSB = 0 },
            new HexBoard { LSB = 0x4004000000004004, MSB = 0 },
            new HexBoard { LSB = 0x1012345678901, MSB = 0 }
            };
            
            Stopwatch stopwatch = new Stopwatch();
            SearchSettings searchSettings = new SearchSettings();
            searchSettings.ThreadedDoubleDepth = false;
            searchSettings.MaxSearchDepth = depth;
            searchSettings.UseIterativeDeepening = true;
            searchSettings.UseThreading = true;

            // First the original.


            // Short tests
            HexBoard shortTest = new HexBoard(1463988815018590998, 0);
            HexBoard followUp = new(1320157434603573762, 0);
            HexBoard fill1 = new(1320157434603573778, 0);
            HexBoard fill2 = new(1320157434603573794, 0);
            string testString = HexBoardActions.PrintHexBoard(shortTest, false);
            Console.WriteLine(testString);
            testString = HexBoardActions.PrintHexBoard(followUp, false);
            Console.WriteLine(testString);
            testString = HexBoardActions.PrintHexBoard(fill1, false);
            Console.WriteLine(testString);
            testString = HexBoardActions.PrintHexBoard(fill2, false);
            Console.WriteLine(testString);
            moveSearcherWorking.StartSearch(shortTest, searchSettings);

            //return;

            stopwatch.Start();
            uint previousTotalNodes = 0;
            int previousTotalEvaluation = 0;
            for (int i = 0; i < 10; i++)
            {
                uint nodes;
                int eval;
                Archive.PreviousVersion.MoveDirection move;
                Archive.PreviousVersion.HexBoard board = new Archive.PreviousVersion.HexBoard { LSB = boardToTestArray[i].LSB , MSB = boardToTestArray[i].MSB };
                (move, nodes, eval) = moveSearcherPreviousVersion.StartSearch(board, 0, depth, true);
                previousTotalEvaluation += eval;
                previousTotalNodes += nodes;

            }
            stopwatch.Stop();
            TimeSpan timePrevious = stopwatch.Elapsed;

            Console.WriteLine($"Previous version:\n" + $"Total Evaluation: {previousTotalEvaluation}, Total nodes {previousTotalNodes}, Time taken: {timePrevious.Seconds}s, {timePrevious.Milliseconds}ms");
            stopwatch.Reset();

            // Then the working, no deepening.
            stopwatch.Start();
            uint workingNoDoubleTotalNodes = 0;
            int workingNoDoubleEvaluation = 0;
            for (int i = 0; i < 10; i++)
            {
                uint nodes;
                int eval;
                MoveDirection move;
                (move, nodes, eval) = moveSearcherWorking.StartSearch(boardToTestArray[i], searchSettings);
                workingNoDoubleEvaluation += eval;
                workingNoDoubleTotalNodes += nodes;
            }
            stopwatch.Stop();
            TimeSpan timeWorkingNoDouble = stopwatch.Elapsed;

            Console.WriteLine($"Working no iterative deepening:\n" + $"Total Evaluation: {workingNoDoubleEvaluation}, Total nodes {workingNoDoubleTotalNodes}, Time taken: {timeWorkingNoDouble.Seconds}s, {timeWorkingNoDouble.Milliseconds}ms.");
            stopwatch.Reset();

            // Then the working, deepening.
            searchSettings.UseIterativeDeepening = true;
            searchSettings.ThreadedDoubleDepth = false;
            searchSettings.UseThreading = false;
            stopwatch.Start();
            uint workingDoubleTotalNodes = 0;
            int workingDoubleEvaluation = 0;
            for (int i = 0; i < 10; i++)
            {
                uint nodes;
                int eval;
                MoveDirection move;
                (move, nodes, eval) = moveSearcherWorking.StartSearch(boardToTestArray[i], searchSettings);
                workingDoubleEvaluation += eval;
                workingDoubleTotalNodes += nodes;
            }
            stopwatch.Stop();
            TimeSpan timeWorkingDouble = stopwatch.Elapsed;

            Console.WriteLine($"Working iterative deepening:\n" + $"Total Evaluation: {workingDoubleEvaluation}, Total nodes {workingDoubleTotalNodes}, Time taken: {timeWorkingDouble.Seconds}s, {timeWorkingDouble.Milliseconds}ms.");
            stopwatch.Reset();


            int evalPrev, evalCur, evalCurDeep;
            MoveDirection moveCur;
            Archive.PreviousVersion.MoveDirection movePrev;
            uint node;
            (movePrev, node, evalPrev) = moveSearcherPreviousVersion.StartSearch(new Archive.PreviousVersion.HexBoard { LSB = 0x2100102, MSB = 0 }, searchSettings);
            searchSettings.UseIterativeDeepening = false;
            searchSettings.ThreadedDoubleDepth = true;
            (moveCur, node, evalCur) = moveSearcherWorking.StartSearch(new HexBoard(0x2100102, 0), searchSettings);
            searchSettings.UseIterativeDeepening = true;
            searchSettings.ThreadedDoubleDepth = false;
            (moveCur, node, evalCurDeep) = moveSearcherWorking.StartSearch(new HexBoard(0x2100102, 0), searchSettings);

            Console.WriteLine($"previous eval: {evalPrev} no deep {evalCur} deep {evalCurDeep} ");





        }

        //[Benchmark]
        public void FirstVersion()
        {
            moveSearcherOriginal.StartSearch(hexBoardToTestOriginal, scoreToTest, depthToTest);
        }

        [Benchmark]
        public void FirstThreadedVersion()
        {
            moveSearcherOriginalThreaded.StartSearch(hexBoardToTestOriginal, scoreToTest, depthToTest, true);
        }

        [Benchmark(Baseline = true)]
        public void PreviousVersion()
        {
            moveSearcherPreviousVersion.StartSearch(hexBoardToTestPrevious, scoreToTest, depthToTest, true);
        }

        [Benchmark]
        public void CurrentVersion()
        {
            moveSearcherWorking.StartSearch(hexBoardToTest, searchSettingsCurrent);
        }



    }
}