﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Scripts
{
    public class CommunicationManager
    {
        

        public void StartCommunications()
        {
            Console.WriteLine("Ready Searcher Started");

            string? received;

            while ((received = Console.ReadLine()) != null)
            {
                string[] subStrings = received.Split(" ");

                switch (subStrings[0])
                {
                    case "Search":
                        {
                            InterpretSearchCommand(subStrings);
                            break;
                        }
                    case "Benchmark":
                        {
                            Benchmarks benchmarks = new Benchmarks();
                            if(subStrings.Length < 2)
                            {
                                benchmarks.InUnityBenchmark(6);
                            }
                            else
                            {
                                benchmarks.InUnityBenchmark(Convert.ToInt32(subStrings[1]));
                            }                           
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

        private void InterpretSearchCommand(string[] commandPartArray)
        {
            // Create settings at default, and empty hexboard.
            SearchSettings searchSettings = new SearchSettings();
            HexBoard hexBoard = new HexBoard();
            // Loop through each command part, and check what kind of command it is.
            foreach (string commandPart in commandPartArray)
            {
                // If it starts with -s:, it's a setting. Each setting starts with a seperate -s:, so this one only contains one setting.
                if (commandPart.StartsWith("-s:"))
                {
                    string[] settingsSplit = commandPart.Split(":");
                    if (settingsSplit[1].StartsWith("ToMaxDepth")) 
                    {
                        searchSettings.SearchToMaxTime = false;
                        searchSettings.MaxSearchDepth = Convert.ToInt32(settingsSplit[2]);
                    }
                    else if (settingsSplit[1].StartsWith("ToMaxTime"))
                    {
                        searchSettings.SearchToMaxTime = true;
                        searchSettings.SearchTimeMillies = Convert.ToInt32(settingsSplit[2]);
                    }
                    else if (settingsSplit[1].StartsWith("ThreadedDouble"))
                    {
                        searchSettings.ThreadedDoubleDepth = true;
                    }
                }
                // If it starts with LSB: or MSB: this contains the HexBoard data.
                if (commandPart.StartsWith("LSB:"))
                {
                    string[] LSBSplit = commandPart.Split(":");
                    hexBoard.LSB = Convert.ToUInt64(LSBSplit[1]);
                }

                if (commandPart.StartsWith("MSB:"))
                {
                    string[] MSBSplit = commandPart.Split(":");
                    hexBoard.MSB = Convert.ToUInt64(MSBSplit[1]);
                }
            }
            // Start the search with the HexBoard and settings obtained from the message.
            SearchStarter searchStarter = new SearchStarter();
            searchStarter.StartSearch(hexBoard, searchSettings);
            

            

        }
    }
}