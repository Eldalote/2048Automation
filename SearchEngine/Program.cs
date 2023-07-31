class program
{
    static void Main(String[] args)
    {
        
        
        Console.WriteLine("Ready Searcher");

        string received;
        while ((received = Console.ReadLine())  != null)
        {
            string[] subStrings = received.Split(" ");
            if (subStrings[0] == "Search")
            {
                Random random = new Random();
                int rand = random.Next(0, 4);
                switch (rand)
                {
                    case 0:
                        {
                            Console.WriteLine("BestMove Up");
                            break;
                        }
                    case 1:
                        {
                            Console.WriteLine("BestMove Down");
                            break;
                        }
                    case 2:
                        {
                            Console.WriteLine("BestMove Right");
                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("BestMove Left");
                            break;
                        }
                        
                }
            }
            else
            {
                Console.WriteLine(received);
            }
        }
        
        
    }
}