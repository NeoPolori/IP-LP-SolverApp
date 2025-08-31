using System;
using System.Threading;

namespace PacmanConsole
{
    public static class VisualEffects
    {
        // Define theme colors for Pac-Man, maze, and ghost
        private static readonly ConsoleColor PacmanYellow = ConsoleColor.Yellow;
        private static readonly ConsoleColor MazeBlue = ConsoleColor.Blue;
        private static readonly ConsoleColor GhostRed = ConsoleColor.Red;

        // ASCII art "Big Font" for rendering letters and numbers (like a blocky banner)
        private static readonly string[][] BigFont = new string[][]
        {
            // Each array inside represents rows of characters (0-9, A-Z, space, etc.)
            new string[]
            {
                "███", "█ █", "███", "███", "█ █", "███", "███", "███", "███", "███", // 0-9
                "   ", // Space
                "█  ", "███", "█ █", "█  ", "█  ", "█  ", "█ █", "███", "  █", // A-I
                " █ ", "█ █", "█  ", "█ █", "█ █", "███", "█ █", "███", "█ █", // J-R
                "███", " ██", "█ █", "█ █", "█ █", "███", "█ █", "███"       // S-Z
            },
            new string[]
            {
                "█ █", "█ █", "█  ", "  █", "█ █", "█  ", "█  ", "  █", "█ █", "█ █",
                "   ",
                "█  ", "█  ", "█ █", "█  ", "██ ", "█  ", "█ █", "  █", "  █",
                "█ █", "█ █", "█  ", "███", "███", "█  ", "█ █", "█  ", "█ █",
                "█  ", " ██", "█ █", "█ █", "█ █", "█ █", "█ █", "█  "
            },
            new string[]
            {
                "█ █", "███", "███", "███", "███", "███", "███", "  █", "███", "███",
                "   ",
                "██ ", "███", "███", "██ ", "██ ", "██ ", "██ ", "  █", "  █",
                "██ ", "██ ", "█  ", "█ █", "█ █", "███", "███", "██ ", "███",
                "███", " ██", "█ █", "█ █", "█ █", "███", "█ █", "███"
            },
            new string[]
            {
                "█ █", "█ █", "█  ", "  █", "█ █", "  █", "█ █", "  █", "█ █", "  █",
                "   ",
                "█  ", "█  ", "█ █", "█  ", "█ █", "█  ", "█ █", "  █", "  █",
                "█ █", "█ █", "█  ", "█  ", "█ █", "█  ", "█ █", "█  ", "█ █",
                "  █", " ██", "█ █", "█ █", "█ █", "█  ", "█ █", "█  "
            },
            new string[]
            {
                "███", "█ █", "███", "███", "█ █", "███", "███", "  █", "███", "███",
                "   ",
                "█  ", "███", "█ █", "███", "█ █", "███", "█ █", "  █", "  █",
                "█ █", "█ █", "███", "█ █", "█ █", "███", "█ █", "███", "█ █",
                "███", " ██", " ██", " ██", " █ ", "███", "█ █", "███"
            }
        };

        // Print large ASCII-style text centered on the screen
        private static void PrintBigCentered(string text, int top, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            int width = Console.WindowWidth;

            // Prepare 5 lines of text (since BigFont is 5 rows high)
            string[] lines = new string[5];
            for (int i = 0; i < 5; i++) lines[i] = "";

            // Build text row by row depending on characters
            foreach (char c in text.ToUpper())
            {
                if (c >= 'A' && c <= 'Z') // Letters
                {
                    int idx = c - 'A' + 11;
                    for (int row = 0; row < 5; row++)
                        lines[row] += BigFont[row][idx] + " ";
                }
                else if (c >= '0' && c <= '9') // Numbers
                {
                    int idx = c - '0';
                    for (int row = 0; row < 5; row++)
                        lines[row] += BigFont[row][idx] + " ";
                }
                else if (c == ' ') // Space
                {
                    for (int row = 0; row < 5; row++)
                        lines[row] += "   ";
                }
                else // Unknown character fallback
                {
                    for (int row = 0; row < 5; row++)
                        lines[row] += "?? ";
                }
            }

            // Print each row centered
            for (int row = 0; row < lines.Length; row++)
            {
                int left = (width / 2) - (lines[row].Length / 2);
                Console.SetCursorPosition(Math.Max(0, left), top + row);
                Console.Write(lines[row]);
            }
        }

        // Show a Pac-Man style animated loading screen
        public static void ShowLoadingScreen()
        {
            Console.Clear();
            Console.CursorVisible = false;
            int height = Console.WindowHeight;
            int width = Console.WindowWidth;

            // Frames for Pac-Man chomping animation
            string[] pacmanFrames = { "C", "◖", "◓", "◑" };
            // Frames for "LOADING" text animation
            string[] frames = { "LOADING", "LOADING.", "LOADING..", "LOADING..." };

            DateTime start = DateTime.Now;
            int frameIndex = 0;

            // Run animation loop for ~1.2 seconds
            while ((DateTime.Now - start).TotalSeconds < 1.2)
            {
                Console.Clear();

                // Pac-Man moves across screen eating dots
                int pacmanX = (frameIndex % (width - 10)) + 5;
                Console.ForegroundColor = PacmanYellow;
                Console.SetCursorPosition(pacmanX, height / 2 - 1);
                Console.Write(pacmanFrames[frameIndex % 4]);

                // Draw trail of dots
                Console.ForegroundColor = ConsoleColor.White;
                for (int x = pacmanX + 2; x < width - 2; x += 2)
                {
                    Console.SetCursorPosition(x, height / 2 - 1);
                    Console.Write(".");
                }

                // Draw big "LOADING" text under animation
                PrintBigCentered(frames[frameIndex % frames.Length], height / 2 + 1, PacmanYellow);

                Thread.Sleep(200); // Delay between frames
                frameIndex++;
            }
            Console.ResetColor();
            Console.Clear();
        }

        // Show main menu with Pac-Man style decorations
        public static void ShowPacmanMenu()
        {
            Console.Clear();
            Console.CursorVisible = false;
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            // Draw outer maze border
            Console.ForegroundColor = MazeBlue;
            for (int x = 0; x < width; x++)
            {
                Console.SetCursorPosition(x, 0); Console.Write(x == 0 ? "╔" : x == width - 1 ? "╗" : "═");
                Console.SetCursorPosition(x, height - 1); Console.Write(x == 0 ? "╚" : x == width - 1 ? "╝" : "═");
            }
            for (int y = 1; y < height - 1; y++)
            {
                Console.SetCursorPosition(0, y); Console.Write("║");
                Console.SetCursorPosition(width - 1, y); Console.Write("║");
            }

            // Draw inner maze-like patterns
            for (int y = 2; y < height - 2; y += 4)
            {
                for (int x = 2; x < width - 2; x += 6)
                {
                    Console.SetCursorPosition(x, y); Console.Write("╠═╦═");
                    Console.SetCursorPosition(x, y + 1); Console.Write("╚═╩═");
                }
            }

            // Scatter dots across screen to look like Pac-Man pellets
            Console.ForegroundColor = ConsoleColor.White;
            for (int y = 3; y < height - 3; y += 2)
            {
                if (y % 4 == 0 || (y - 1) % 4 == 0) continue;
                for (int x = 3; x < width - 3; x += 3)
                {
                    if (x % 6 == 2 || x % 6 == 3 || x % 6 == 4) continue;
                    Console.SetCursorPosition(x, y);
                    Console.Write(".");
                }
            }

            // Title with Pac-Man and ghost
            Console.ForegroundColor = PacmanYellow;
            Console.SetCursorPosition(width / 2 - 10, 2); Console.Write("C"); // Pac-Man
            Console.ForegroundColor = GhostRed;
            Console.SetCursorPosition(width / 2 + 8, 2); Console.Write("👻"); // Ghost
            PrintBigCentered("LINEAR PROGRAMMING", 4, PacmanYellow);

            // Display menu options with dot borders
            string[] options = {
                "1. Solve Knapsack Problem",
                "2. Perform Sensitivity Analysis",
                "3. Primal Simplex",
                "4. Exit"
            };

            int menuTop = height / 2 - 2;
            Console.ForegroundColor = PacmanYellow;
            for (int i = 0; i < options.Length; i++)
            {
                int left = (width / 2) - (options[i].Length / 2 + 4);
                Console.SetCursorPosition(left, menuTop + (i * 2));
                Console.Write("... " + options[i] + " ...");
            }

            // Input prompt centered below menu
            string prompt = "... Please choose an option: ";
            int promptLeft = (width / 2) - (prompt.Length / 2);
            Console.SetCursorPosition(promptLeft, menuTop + (options.Length * 2) + 2);
            Console.Write(prompt);

            Console.ResetColor();
        }
    }
}
