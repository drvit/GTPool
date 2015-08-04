using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTPool.Sandbox;

namespace GTPool.App.ThreadExercises
{
    public class RunExercises
    {
        public static void WhatExercise()
        {
            bool exit;

            do
            {
                Console.Clear();

                Console.Write("What exercise to Run? [1, 2, 3, 4, 5, 6, 7, 71, 8, 9, 91, 10, 11, 12]: ");
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
                        case 11:
                            Exercise11.Run();
                            break;
                        case 12:
                            Exercise12.Run();
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

                Console.WriteLine();
                Console.WriteLine("....................................");
                Console.Write("Exit? (Y) ");
                exit = string.Compare(Console.ReadLine(), "Y", StringComparison.InvariantCultureIgnoreCase) >= 0;

            } while (!exit);
        }
    }
}
