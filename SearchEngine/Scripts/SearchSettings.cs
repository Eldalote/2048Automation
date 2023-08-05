using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Scripts
{
    public class SearchSettings
    {
        public bool ThreadedDoubleDepth;
        public int SearchDepth;
        public bool SearchToMaxTime;
        public int SearchTimeMillies;
        public bool UseIterativeDeepening;
        public bool UseThreading;
        public int ThreadSplitDepth;
        public int MaxDepthIterativeDeepening;


        public SearchSettings() 
        {
            ThreadedDoubleDepth = false;
            SearchDepth = 8;
            SearchToMaxTime = false;
            SearchTimeMillies = 600;
            UseIterativeDeepening = true;
            UseThreading = false;
            ThreadSplitDepth = 2;
            MaxDepthIterativeDeepening = 35;
        }
    }
}
