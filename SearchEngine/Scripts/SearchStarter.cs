using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Scripts
{
    public class SearchStarter
    {


        public void StartSearch(HexBoard board, SearchSettings settings)
        {
            Task task = Task.Factory.StartNew(() => ActualStarter(board, settings));
        }


        public void StopSearch() 
        {

        }


        private void ActualStarter(HexBoard board, SearchSettings settings)
        {
            MoveSearchMiniMax moveSearcher = new MoveSearchMiniMax();
            MoveDirection direction;
            uint nodesSearched;
            int evaluation;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            (direction, nodesSearched, evaluation) = moveSearcher.StartSearch(board, settings);
            stopWatch.Stop();
            TimeSpan timeSpan = stopWatch.Elapsed;
            string resultString = "BestMove " + direction.ToString() + " ";
            if (settings.SearchToMaxTime)
            {
                resultString += $"Searched to time: {settings.SearchTimeMillies}ms. ";
            }
            else
            {
                resultString += $"Searched to depth: {settings.SearchDepth}. ";
            }
            resultString += $"Nodes: {nodesSearched}, Evaluation: {evaluation}, Time: {timeSpan.Seconds}s, {timeSpan.Milliseconds}ms.";
            Console.WriteLine(resultString);
        }
    }
}
