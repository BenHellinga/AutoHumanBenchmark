using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HumanBenchmark
{
    /*
    * This has all the code to automatically complete the 'chimp test'
    * 
    * When run, asks for 2 points on the screen that mark out a rectangle that contains the game board inside.
    * These points can be anywhere, however, the topleft point MUST be within a single gametile from the topleft game tile
    * Otherwise the piece detection fricks up
    * 
    * It takes a screenshot of the gameboard, locates the coordinates of all 40 game tiles, and uses the screenshots in assets/chimptest
    * to find what number that tile is. Then stores the coordinates in an array at that numbers location, and once it has
    * read all the tiles, it just clicks at the coordinates stored in the array in order from 0 to n, where n is what stage you're on
    * 
    */

    internal class ChimpTestPlayer
    {

        // CONSTANTS
        // ==================================================

        // if you don't have all the number assets, this will screenshot any number it does not recognize
        // and save it as the number that should be missing
        //
        // you do need to have at least the first 3 numbers
        private const bool MAKE_NEW_SAVES = true;

        // file path and file name
        private const String FILEPATH = "../../assets/chimptest/";
        private const String FILENAME = "chimptest";
        private const String FILEEND = ".png";

        // this is the offset from the topleft corner of each tile to where the screenshots are
        //
        // the screenshots arn't the full number because that's slow, so only the middle sliver is needed
        //
        // this offsets from the topleft of each tile to the topleft of the sliver
        private const int GRID_OFFSET_X = 5;
        private const int GRID_OFFSET_Y = 27;

        // tile size
        private const int GRID_SPACING_X = 90;
        private const int GRID_SPACING_Y = 90;

        // board size
        private const int GRID_WIDTH = 8;
        private const int GRID_HEIGHT = 5;

        // where to click on the tile offset
        private const int CLICK_OFFSET_X = 20;
        private const int CLICK_OFFSET_Y = 0;

        // clicking timing
        //
        // if it's too fast it messes up. maximum speed i've found is 10/30, which is what it is set too when FAST = true
        // otherwise, it's whatever you wanna set it too
        private const bool FAST = false;
        private const int CLICK_DELAY = FAST ? 10 : 100; // 10
        private const int MENU_DELAY = FAST ? 30 : 1000; // 20

        // this is how large to save the new assets if numbers are missing
        private const int SAVE_WIDTH = 70;
        private const int SAVE_HEIGHT = 6;

        // this is where to click the continue button
        private const int CONTINUE_OFFSET_X = 0;
        private const int CONTINUE_OFFSET_Y = 50;


        // VARIABLES
        // ==================================================

        // this is responsible for screenshotting the screen
        public static ScreenReader s = new ScreenReader();

        // stores the screenshot and locations of the identified numbers
        public static List<Bitmap> numbers;
        public static Point[] numberLocations;

        // the color of the outline around the tiles to find if there is a tile at that location
        public static Color outlineColor = Color.FromArgb(255, 65, 147, 214);

        // the x and y offset of where the game is relative to where you set the topleft corner with the cursor
        public static int game_x;
        public static int game_y;

        //
        public static int continue_x;
        public static int continue_y;

        // stores the stage and whether all the numbers have been found or not
        public static bool foundAll;
        public static int stage;


        // PUBLIC METHODS
        // ==================================================

        // the main function that runs the autoplayer
        public static void playChimpTest()
        {
            //initNumberPictures();

            s.getRectangleViaCursor(); // get the top-left and bottom-right corner approximations with the cursor

            Console.Write("Press enter to start");
            Console.ReadLine();

            s.captureScreenshot();
            locateGrid(); // scans the screenshot for tiles and finds displacement of the actual game from the top-left corner approximation

            // set expected cursor position
            //
            // this is for pausing the program. If the mouse is not in the place it is expected, it means you've moved the mouse
            // and the program can pause
            CursorManager.expectedCursorPosition = Cursor.Position;

            stage = 4; // the game starts on stage 4
            while (true)
            {
                if (stage == 41) break; // the game ends on stage 41

                s.captureScreenshot();

                if (CursorManager.cursorMoved())
                {
                    Console.Write("Cursor moved. Press enter to start again"); // pause if cursor is moved by user
                    Console.ReadLine();
                }

                // reads the screenshots in assets into a list of bitmaps
                // this happens every stage, but its fast enough anyways so it doesnt matter that much
                initNumberPictures();

                // find the location of numbers
                if (!findNumberLocations(stage))
                    break;

                // click them in order from 1 - 40
                clickNumbers();

                // delay
                Thread.Sleep(MENU_DELAY);
                CursorManager.leftClick(continue_x, continue_y);
                Thread.Sleep(MENU_DELAY);

                if (foundAll)
                    stage++;
            }

            Console.WriteLine("Finished");
            Console.ReadLine();
        }




        // PRIVATE METHODS
        // ==================================================

        // this reads in the assets into a list of bitmaps for comparison with screenshots taken of the game

        private static void initNumberPictures()
        {
            numbers = new List<Bitmap>();

            int i = 1;
            while (true)
            {
                try
                {
                    numbers.Add(new Bitmap(FILEPATH + FILENAME + i + FILEEND));
                }
                catch (Exception e) // definitely not a good fix lol
                {
                    e.Source = null;
                    break;
                }
                i++;
            }
        }



        // searches 1 pixel on the board of all 40 tile locations. If the color there matches the boardcolor,
        // then there is a number there
        // 
        // loop through all the bitmaps in 'numbers' and find a match
        //
        // store the location of the tile in the array of points at the index of the match

        private static bool findNumberLocations(int max)
        {
            int x, y;
            bool found;

            numberLocations = new Point[max];
            foundAll = false;

            // double loop to loop through all possible tile locations
            for (int grid_x = 0; grid_x < GRID_WIDTH; grid_x++)
            {
                for (int grid_y = 0; grid_y < GRID_HEIGHT; grid_y++)
                {
                    x = game_x + grid_x * GRID_SPACING_X; // calculate the x and y of a specific tile
                    y = game_y + grid_y * GRID_SPACING_Y;

                    if (!s.screenshot.GetPixel(x - 1, y).Equals(outlineColor)) continue; // if the color there is not the border color, skip

                    found = false;
                    for (int i = 0; i < numbers.Count; i++) // loop through all the screenshots in assets and find the matching number
                    {
                        if (!s.searchScreenShot(numbers[i], x, y)) continue; // if it doesn't match, skip to next asset

                        if (i == stage - 1) foundAll = true;

                        numberLocations[i].X = x; // if it's found, store the location and skip to the next tile location
                        numberLocations[i].Y = y;

                        found = true;
                        break;
                    }

                    if (found) continue;

                    if (!MAKE_NEW_SAVES) // if not found but there is a number there, save the new number as the last number possible
                    {
                        Console.WriteLine("No match found");
                        return false;
                    }

                    ScreenReader reader = new ScreenReader(x + s.x, y + s.y, SAVE_WIDTH, SAVE_HEIGHT);
                    reader.captureScreenshot();
                    reader.saveScreenShot(FILEPATH + FILENAME + (numbers.Count + 1) + FILEEND);

                    numberLocations[numbers.Count].X = x;
                    numberLocations[numbers.Count].Y = y;
                }
            }

            return true;

        }



        // uses the coordinate approximations given by the user via the cursor to find the 
        // actuall location of the grid
        //
        // scans through every pixel until it finds the board color of a tile, then uses that location
        // relative to the topleft corner provided by the user modulus the width of the tile
        //
        // this is becuase the tiles can be anywhere randomly in the gameboard

        private static void locateGrid()
        {
            for (int x = 0; x < s.screenshot.Width; x++) // scan through every pixel until you find border color
            {
                for (int y = 0; y < s.screenshot.Height; y++)
                {
                    if (s.screenshot.GetPixel(x, y).Equals(outlineColor)) // if you find border color, locate game_x/y relative to provided topleft corner
                    {
                        game_x = (x % GRID_SPACING_X) + GRID_OFFSET_X;
                        game_y = (y % GRID_SPACING_Y) + GRID_OFFSET_Y;

                        continue_x = s.x + game_x + (GRID_WIDTH / 2) * GRID_SPACING_X; // this locates the position of the continue button
                        continue_y = s.y + game_y + (GRID_HEIGHT - 2) * GRID_SPACING_Y + CONTINUE_OFFSET_Y;

                        return;
                    }
                }
            }
        }



        // this just reads the array of positions in order and clicks at those locations

        private static void clickNumbers()
        {
            for (int i = 0; i < numberLocations.Length; i++)
            {
                int x, y;

                x = numberLocations[i].X + s.x + CLICK_OFFSET_X; // click offset to click the middle of the tile
                y = numberLocations[i].Y + s.y + CLICK_OFFSET_Y;

                CursorManager.leftClick(x, y);
                Thread.Sleep(CLICK_DELAY);
            }
        }
    }
}
