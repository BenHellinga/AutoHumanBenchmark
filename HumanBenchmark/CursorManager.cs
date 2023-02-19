using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HumanBenchmark
{
    internal class CursorManager
    {

        // VARIABLES

        public static Point expectedCursorPosition = Point.Empty;



        // EXTERNAL METHODS

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;



        // PUBLIC METHODS

        public static bool cursorMoved()
        {
            if (Cursor.Position.Equals(Point.Empty))
            {
                Console.WriteLine("Unkown cursror position");
                return true;
            }

            if (Cursor.Position.X != expectedCursorPosition.X ||
                Cursor.Position.Y != expectedCursorPosition.Y)
                return true;
            return false;
        }



        public static void leftClick(int x, int y)
        {
            expectedCursorPosition = new Point(x, y);
            Cursor.Position = expectedCursorPosition;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
        }

        public static void leftClick(Point point)
        {
            leftClick(point.X, point.Y);
        }




        public static void getPosition(ref Point point)
        {
            GetCursorPos(ref point);
        }



        public static Point findColorNearCursor(Color color, int width, int height)
        {
            Console.Write("Press enter to save cursor position");
            Console.ReadLine();

            Point point = Cursor.Position;

            Console.Write("Press enter to search for color");
            Console.ReadLine();

            ScreenReader s = new ScreenReader(point.X - width / 2, point.Y - height / 2, width, height);

            s.captureScreenshot();
            Point position = s.findColor(color);

            point.X += position.X - 50;
            point.Y += position.Y - 50;

            return point;
        }










    }
}
