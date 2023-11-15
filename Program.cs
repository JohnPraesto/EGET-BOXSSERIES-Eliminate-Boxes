using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Data.Common;

namespace Eliminate_Boxes
{
    internal class Program
    {
        private static int boxesCaptured = 0;
        private static int boxCount = 0;
        private static int oX = 20;
        private static int oY = 10;
        private static int x = 0;
        private static int y = 0;
        private static int interval = 2000;
        private static int[] oPos = new int[2];                 // This is an array with two int elements that will store the coordinates for the player character "O"
        private static List<int[]> posList = new List<int[]>(); // This list will store coodinates for spawned boxes
        private static Thread spawnBox = new Thread(SpawnBox);  // This Thread will spawn boxes through SpawnBox method
        private static Stopwatch clock = new Stopwatch();
        private static List<string[]> topList = new List<string[]>();

        static void Main(string[] args)
        {
            PrintMenu();
            ConsoleKeyInfo oMove; // This variable will be used to read the user input (user will press arrow keys)
            spawnBox.Start();     // Starts the separate thread to run simultaneously as Main
            clock.Start();        // Starts the timer that will be used to increase the spawnrate of the boxes

            while (true)
            {
                oMove = Console.ReadKey();
                Console.SetCursorPosition(oX, oY);  // Setting the cursor to the position of the player (cursor will be left of the player character "O")
                Console.Write(" ");                 // Overwrites the previous position of the player
                if (oMove.Key == ConsoleKey.UpArrow)//Reading user input to update cursor position
                    oY--;
                else if (oMove.Key == ConsoleKey.RightArrow)
                    oX++;
                else if (oMove.Key == ConsoleKey.DownArrow)
                    oY++;
                else if (oMove.Key == ConsoleKey.LeftArrow)
                    oX--;
                CheckBoundry(ref oX, ref oY);
                Console.SetCursorPosition(oX, oY);  // Sets new cursor position for the player
                oPos[0] = oX;
                oPos[1] = oY;
                Console.Write("O");                 // Writes the player character "O" in the new curson position
                int elementCount = 0;

                // Checking if the current position of the player matches any of the positions of boxes on the field (stored in posList)
                // If they match that position will be removed from posList (The player has eliminated a box)
                foreach (int[] item in posList)
                {
                    if (item[0] == oPos[0])
                    {
                        if (item[1] == oPos[1])
                        {
                            boxCount--;
                            boxesCaptured++;
                            DisplayBoxCount();
                            posList.RemoveAt(elementCount);
                            break;
                        }
                    }
                    elementCount++;
                }
            }
        }
        public static void SpawnBox()
        {
            Random rnd = new Random();
            int[] boxPos;
            double time;
            while (true)
            {
                x = rnd.Next(0, 41); // Boxes can only spawn randomly within the field limits 40x20
                y = rnd.Next(0, 21);
                Console.SetCursorPosition(x, y);
                boxPos = new int[2];
                boxPos[0] = x;
                boxPos[1] = y;
                posList.Add(boxPos); // Adds a new position to the posList
                Console.Write("■");
                Console.Beep();
                boxCount++;
                if (boxCount > 10)
                {
                    Console.Clear();
                    Console.WriteLine($"\nYou captured {boxesCaptured} boxes!");

                    Record(boxesCaptured, topList);

                    Console.Write("\nPress Enter to return to menu!");
                    Console.ReadLine();
                    interval = 2000;
                    oX = 20;
                    oY = 10;
                    boxesCaptured = 0;
                    boxCount = 0;
                    posList.Clear();
                    PrintMenu();
                }
                DisplayBoxCount();

                // NOTE: Intervals are not updated evenly. TO BE SOLVED.
                // For each 10 seconds the box spawn rate will increase. (Spawn interval reduced 150 milliseconds every 10 seconds).
                time = clock.ElapsedMilliseconds / 1000; 
                if (time % 10 == 0)
                    interval -= 150;
                Thread.Sleep(interval);
            }
        }
        public static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("Chase the boxes using the arrow keys!");
            Console.WriteLine("If there are more than 10 boxes on the field you lose the game!");
            Console.WriteLine("\nPress Enter to Start\n");
            topList.Clear();
            if (File.Exists("Record.txt"))
            {
                StreamReader reader = new StreamReader("Record.txt");

                while (reader.Read() > 0)
                {
                    string row = reader.ReadLine();

                    // ### is used as separator in the textfile. Here we split the strings from the textfile
                    // and store remaining strings in an array that will be added to the record holders topList
                    string[] array = row.Split(new string[] { "###" }, StringSplitOptions.None);
                    topList.Add(array);
                }
                reader.Close();

                int i = 1;

                topList.Sort(new LastElementComparer());

                Console.WriteLine("\n=============== RECORD HOLDERS ================");
                Console.WriteLine("   NAME           DATE                    BOXES");
                foreach (string[] item in topList)
                {
                    Console.WriteLine($"{i}. {item[0],-15}{item[1],-24}{item[2]}");
                    i++;
                }
            }
            Console.ReadLine();
            Console.Clear();
        }

        // So that the player cannot move outside the playing field
        public static void CheckBoundry(ref int x, ref int y)
        {
            if (x > 40)
                x = 40;
            if (x < 0)
                x = 0;
            if (y > 20)
                y = 20;
            if (y < 0)
                y = 0;
        }
        public static void DisplayBoxCount()
        {
            Console.SetCursorPosition(50, 0);
            if(boxCount < 6)
                Console.ForegroundColor = ConsoleColor.Green;
            else if(boxCount < 9)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else
                Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.Write($"Box count: {boxCount}");
            Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write(interval);
        }
        public static void Record(int boxesCaptured, List<string[]> topList)
        {
            int isTop10 = 0;
            int rowsInRecordTxt = 0;

            // topList has been loaded with array of strings taken from the textfile Records.txt
            // This loop compares the number of boxes captured/eliminated with earlier recorded captured number boxes stored in the list
            foreach (string[] item in topList)
            {
                if(boxesCaptured >= Convert.ToInt32(item[2]))
                {
                    isTop10++;
                }
                rowsInRecordTxt++;
            }
            if(isTop10 > 0 || rowsInRecordTxt < 11) // If the number of boxes captured is more than any of the other number of boxes captured in the list
                                                    // or if there are 10 or less posts in the top 10 list
                                                    // that means the player made the top 10 list
            {
                Console.WriteLine($"You're in the top 10! You are nr {rowsInRecordTxt+1-isTop10} in top list!");
                Console.Write("Write your name: ");
                string name = Console.ReadLine();

                StreamWriter writer = new StreamWriter("Record.txt");

                DateTime now = DateTime.Now;

                string[] putInList = new string[3] { name, now.ToString(), boxesCaptured.ToString() };
                topList.Add(putInList); // Adds a new string array to the topList
                topList.Sort(new LastElementComparer()); // Sorting the list highest to lowes box count

                if(topList.Count > 10) 
                {
                    topList.RemoveAt(10); // Deleting the last element of the list (the one with least box count)
                }

                foreach (string[] item in topList) // Re-writing the textfile with updated content of topList
                {
                    writer.WriteLine("." + item[0] + "###" + item[1] + "###" + item[2]);
                }
                writer.Close();
            }
        }
    }

    // This class compares the last elements if the arrays in the topList
    // ChatGPT did this one, I dont really understand it.
    public class LastElementComparer : IComparer<string[]>
    {
        public int Compare(string[] x, string[] y)
        {
            int lastElementX = int.Parse(x[x.Length - 1]);
            int lastElementY = int.Parse(y[y.Length - 1]);

            return lastElementY.CompareTo(lastElementX);
        }
    }
}