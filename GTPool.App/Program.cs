using System;
using GTPool.App.ThreadExercises;

namespace GTPool.App
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("What exercise to Run? (1-9): ");
            var exercise = Console.ReadLine();
            int n;

            if (int.TryParse(exercise, out n))
            {
                switch (n)
                {
                    case 1:
                        Exercise1.Run();
                        break;
                    case 2:
                        Exercise2.Run();
                        break;
                    case 3:
                        Exercise3.Run();
                        break;
                    case 4:
                        Exercise4.Run();
                        break;
                    case 5:
                        Exercise5.Run();
                        break;
                    case 6:
                        Exercise6.Run();
                        break;
                    case 7:
                        Exercise7.Run();
                        break;
                    case 71:
                        Exercise71.Run();
                        break;
                    case 8:
                        Exercise8.Run();
                        break;
                    case 9:
                        Exercise9.Run();
                        break;
                    case 91:
                        Exercise91.Run();
                        break;
                    case 10:
                        Exercise10.Run();
                        break;
                    default:
                        Console.WriteLine("Wrong exercise number!");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input! Try again...");
            }
        }
    }
}
