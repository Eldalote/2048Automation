using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CommunicationManager
{


    public void StartCommunications()
    {
        Console.WriteLine("Ready Searcher Started");

        string received;

        while ((received = Console.ReadLine()) != null)
        {
            string[] subStrings = received.Split(" ");

            switch (subStrings[0])
            {
                case "Search":
                    {
                        InterpretSearchCommand(received);
                        break;
                    }
                case "Benchmark":
                    {
                        Benchmarks benchmarks = new Benchmarks();
                        benchmarks.InUnityBenchmark();
                        Console.WriteLine("Ready Benchmark Done");
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Message received: {received}");
                        break; 
                    }
            }            
        }
    }

    private void InterpretSearchCommand(string command)
    {
        string[] subString = command.Split(" ");

    }
}

