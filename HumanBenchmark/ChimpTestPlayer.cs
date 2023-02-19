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
    internal class ChimpTestPlayer
    {

        // CONSTANTS

        private const bool MAKE_NEW_SAVES = true;

        private const String FILEPATH = "../../assets/";
        private const String FILENAME = "chimptest";
        private const String FILEEND = ".png";

        private const int GRID_OFFSET_X = 5;
        private const int GRID_OFFSET_Y = 27;

        private const int GRID_SPACING_X = 90;
        private const int GRID_SPACING_Y = 90;

        private const int GRID_WIDTH = 8;
        private const int GRID_HEIGHT = 5;

        private const int CLICK_OFFSET_X = 20;
        private const int CLICK_OFFSET_Y = 0;

        private const bool FAST = true;
        private const int CLICK_DELAY = FAST ? 10 : 100; // 10
        private const int MENU_DELAY = FAST ? 30 : 1000; // 20

        private const int SAVE_WIDTH = 70;
        private const int SAVE_HEIGHT = 6;

        private const int CONTINUE_OFFSET_X = 0;
        private const int CONTINUE_OFFSET_Y = 50;


        // VARIABLES

        public static ScreenReader s = new ScreenReader();

        public static List<Bitmap> numbers;
        public static Point[] numberLocations;

        public static Color outlineColor = Color.FromArgb(255, 65, 147, 214);

        public static int game_x;
        public static int game_y;

        public static int continue_x;
        public static int continue_y;

        public static bool foundAll;
        public static int stage;


        // PUBLIC METHODS

        public static void playChimpTest()
        {
            //initNumberPictures();

            s.getRectangleViaCursor();

            Console.Write("Press enter to start");
            Console.ReadLine();

            s.captureScreenshot();
            locateGrid();

            CursorManager.expectedCursorPosition = Cursor.Position;

            stage = 4;
            while (true)
            {
                if (stage == 41) break;

                s.captureScreenshot();
                //s.saveScreenShot("screenshot.png");
                //break;

                if (CursorManager.cursorMoved())
                {
                    Console.Write("Cursor moved. Press enter to start again");
                    Console.ReadLine();
                }

                initNumberPictures();

                if (!findNumberLocations(stage))
                    break;

                clickNumbers();

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
                catch (Exception e)
                {
                    e.Source = null;
                    break;
                }
                i++;
            }
        }



        private static bool findNumberLocations(int max)
        {
            int x, y;
            bool found;

            numberLocations = new Point[max];
            foundAll = false;

            for (int grid_x = 0; grid_x < GRID_WIDTH; grid_x++)
            {
                for (int grid_y = 0; grid_y < GRID_HEIGHT; grid_y++)
                {
                    x = game_x + grid_x * GRID_SPACING_X;
                    y = game_y + grid_y * GRID_SPACING_Y;

                    if (!s.screenshot.GetPixel(x - 1, y).Equals(outlineColor)) continue;

                    found = false;
                    for (int i = 0; i < numbers.Count; i++)
                    {
                        if (!s.searchScreenShot(numbers[i], x, y)) continue;

                        if (i == stage - 1) foundAll = true;

                        numberLocations[i].X = x;
                        numberLocations[i].Y = y;

                        found = true;
                        break;
                    }

                    if (found) continue;

                    if (!MAKE_NEW_SAVES)
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



        private static void locateGrid()
        {
            for (int x = 0; x < s.screenshot.Width; x++)
            {
                for (int y = 0; y < s.screenshot.Height; y++)
                {
                    if (s.screenshot.GetPixel(x, y).Equals(outlineColor))
                    {
                        game_x = (x % GRID_SPACING_X) + GRID_OFFSET_X;
                        game_y = (y % GRID_SPACING_Y) + GRID_OFFSET_Y;

                        continue_x = s.x + game_x + (GRID_WIDTH / 2) * GRID_SPACING_X;
                        continue_y = s.y + game_y + (GRID_HEIGHT - 2) * GRID_SPACING_Y + CONTINUE_OFFSET_Y;

                        return;
                    }
                }
            }
        }



        private static void clickNumbers()
        {
            for (int i = 0; i < numberLocations.Length; i++)
            {
                int x, y;

                x = numberLocations[i].X + s.x + CLICK_OFFSET_X;
                y = numberLocations[i].Y + s.y + CLICK_OFFSET_Y;

                CursorManager.leftClick(x, y);
                Thread.Sleep(CLICK_DELAY);
            }
        }
    }
}
