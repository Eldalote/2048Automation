using BenchmarkDotNet.Running;
using SearchEngine.Scripts;

class Program
{
    static void Main(String[] args)
    {

        if (args.Length == 0)
        {
            CommunicationManager communicationManager = new CommunicationManager();
            communicationManager.StartCommunications();
        }
        else
        {
            foreach (string arg in args)
            {
                if (arg == "BenchmarkDotNet")
                {
                    BenchmarkRunner.Run<Benchmarks>();
                }
            }
        }

        
    }
}