using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 
 * All this is gunna be is menu system that calls the required methods to run each game
 * 
 * Theres gunna be a class for each game
 * 
 * No inputs
 * No outputs
 * 
 */

namespace HumanBenchmark
{
    internal class GameMenu
    {

        static void Main()
        {
            Console.WriteLine("Playing Chimp Test"); // only this is completed
            ChimpTestPlayer.playChimpTest();
        }


    }
}
