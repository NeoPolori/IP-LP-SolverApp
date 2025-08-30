using System;
using System.Threading;

namespace PacmanConsole
{
    public static class VisualEffects
    {
        private static readonly ConsoleColor PacmanYellow = ConsoleColor.Yellow;
        private static readonly ConsoleColor MazeBlue = ConsoleColor.Blue;
        private static readonly ConsoleColor GhostRed = ConsoleColor.Red;

        // Large ASCII font (blocky, bold letters for numbers and custom PAC-MAN letters)
        private static readonly string[][] BigFont = new string[][]
        {
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

        // Helper: render BIG centered text (ASCII style)
        private static void PrintBigCentered(string text, int top, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            int width = Console.WindowWidth;

            string[] lines = new string[5]; // 5 rows tall
            for (int i = 0; i < 5; i++) lines[i] = "";

            foreach (char c in text.ToUpper())
            {
                if (c >= 'A' && c <= 'Z')
                {
                    int idx = c - 'A' + 11; // Letters start at index 11
                    for (int row = 0; row < 5; row++)
                        lines[row] += BigFont[row][idx] + " ";
                }
                else if (c >= '0' && c <= '9')
                {
                    int idx = c - '0';
                    for (int row = 0; row < 5; row++)
                        lines[row] += BigFont[row][idx] + " ";
                }
                else if (c == ' ')
                {
                    for (int row = 0; row < 5; row++)
                        lines[row] += "   ";
                }
                else
                {
                    for (int row = 0; row < 5; row++)
                        lines[row] += "?? ";
                }
            }

            for (int row = 0; row < lines.Length; row++)
            {
                int left = (width / 2) - (lines[row].Length / 2);
                Console.SetCursorPosition(Math.Max(0, left), top + row);
                Console.Write(lines[row]);
            }
        }

        // Loading screen with Pac-Man chasing dots animation
        public static void ShowLoadingScreen()
        {
            Console.Clear();
            Console.CursorVisible = false;
            int height = Console.WindowHeight;
            int width = Console.WindowWidth;

            string[] pacmanFrames = { "C", "◖", "◓", "◑" }; // Pac-Man chomping animation
            string[] frames = { "LOADING", "LOADING.", "LOADING..", "LOADING..." };

            DateTime start = DateTime.Now;
            int frameIndex = 0;
            while ((DateTime.Now - start).TotalSeconds < 1.2)
            {
                Console.Clear();
                // Pac-Man chasing dots
                int pacmanX = (frameIndex % (width - 10)) + 5;
                Console.ForegroundColor = PacmanYellow;
                Console.SetCursorPosition(pacmanX, height / 2 - 1);
                Console.Write(pacmanFrames[frameIndex % 4]);
                Console.ForegroundColor = ConsoleColor.White;
                for (int x = pacmanX + 2; x < width - 2; x += 2)
                {
                    Console.SetCursorPosition(x, height / 2 - 1);
                    Console.Write(".");
                }

                // Loading text
                PrintBigCentered(frames[frameIndex % frames.Length], height / 2 + 1, PacmanYellow);
                Thread.Sleep(200);
                frameIndex++;
            }
            Console.ResetColor();
            Console.Clear();
        }

        // Draw enhanced Pac-Man themed menu
        public static void ShowPacmanMenu()
        {
            Console.Clear();
            Console.CursorVisible = false;
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            // Draw maze-like border with corners
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
            // Inner maze-like lines
            for (int y = 2; y < height - 2; y += 4)
            {
                for (int x = 2; x < width - 2; x += 6)
                {
                    Console.SetCursorPosition(x, y); Console.Write("╠═╦═");
                    Console.SetCursorPosition(x, y + 1); Console.Write("╚═╩═");
                }
            }

            // Dots across screen for game-like feel, avoiding maze lines
            Console.ForegroundColor = ConsoleColor.White;
            for (int y = 3; y < height - 3; y += 2)
            {
                if (y % 4 == 0 || (y - 1) % 4 == 0) continue; // Skip rows with maze lines
                for (int x = 3; x < width - 3; x += 3)
                {
                    if (x % 6 == 2 || x % 6 == 3 || x % 6 == 4) continue; // Skip columns with maze lines
                    Console.SetCursorPosition(x, y);
                    Console.Write(".");
                }
            }

            // Big Title with Pac-Man and Ghost
            Console.ForegroundColor = PacmanYellow;
            Console.SetCursorPosition(width / 2 - 10, 2); Console.Write("C");
            Console.ForegroundColor = GhostRed;
            Console.SetCursorPosition(width / 2 + 8, 2); Console.Write("👻");
            PrintBigCentered("LINEAR PROGRAMMING", 4, PacmanYellow);

            // Menu options with bold text and dot trails
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

            // Input prompt with dots
            string prompt = "... Please choose an option: ";
            int promptLeft = (width / 2) - (prompt.Length / 2);
            Console.SetCursorPosition(promptLeft, menuTop + (options.Length * 2) + 2);
            Console.Write(prompt);

            Console.ResetColor();
        }
    }
}