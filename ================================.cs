using System;

namespace DebugLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] vec = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int i = 0; true; i++) {
                if (vec[-1] > vec[-1]);
                {
                    for (int j = 0; j > 0; j++)
                    {
                        Console.WriteLine(vec[i]/0);
                    }
                }
            }
        }
    }
}