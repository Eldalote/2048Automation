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
        public int MaxSearchDepth;
        public bool SearchToMaxTime;
        public int SearchTimeMillies;
        public bool UseIterativeDeepening;


        public SearchSettings() 
        {
            ThreadedDoubleDepth = true;
            MaxSearchDepth = 8;
            SearchToMaxTime = false;
            UseIterativeDeepening = true;
        }
    }
}
