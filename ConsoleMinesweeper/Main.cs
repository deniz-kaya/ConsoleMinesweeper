using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ConsoleMinesweeper
{
    internal class Wrapper
    {

        static void Main()
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(49, 32);
            Console.SetBufferSize(49, 32);
            
            //Console.WindowWidth = 49;
            //Console.WindowHeight = 32;
            Menu.Element[] difficultyElements =
                {
                    new Menu.Element{ text = "Easy",foreground = ConsoleColor.Green },
                    new Menu.Element{ text = "Medium", foreground = ConsoleColor.Red},
                    new Menu.Element{ text = "Hard", foreground = ConsoleColor.DarkRed },
                    new Menu.Element{ text = "Insane", foreground = ConsoleColor.Magenta },
                    new Menu.Element{ text = "Delusion", foreground = ConsoleColor.DarkMagenta}

                };
            ConsoleColor cursor, foreground, background;
            cursor = ConsoleColor.DarkCyan; foreground = ConsoleColor.Black; background = ConsoleColor.Gray;
            Menu difficultyMenu = new Menu("Difficulty", true, difficultyElements, (2, 1), ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.DarkGray) ;
            int difficulty = difficultyMenu.OperateMenu();
            Minesweeper game;
            switch (difficulty)
            {
                case 0:
                    game = new Minesweeper(10, 8, 10, cursor,foreground,background);
                    break;
                case 1:
                    game = new Minesweeper(18, 14, 40, cursor, foreground, background);
                    break;
                case 2:
                    game = new Minesweeper(24, 20, 99, cursor, foreground, background);
                    break;
                case 3:
                    game = new Minesweeper(30, 25, 150, cursor, foreground, background);
                    break;
                case 4:
                    game = new Minesweeper(40, 30, 300, cursor, foreground, background);
                    break;
                default:
                    game = new Minesweeper(4,4,1,ConsoleColor.Yellow, ConsoleColor.Black, ConsoleColor.White);
                    break;
            }
            while (game.GameState)
            {
                game.GameTick();  
                //game.DebugPrintBoard();
            }
            game.GameOverSequence();
        }
    }
}
/*switch (diff)
{
    case 0:
        gameGrid = new(8, 10, 10);
        break;
    case 1:
        gameGrid = new(14, 18, 40);
        break;
    case 2:
        gameGrid = new(20, 24, 99);
        break;
    case 4:
        gameGrid = new(30, 30, 175);
        break;
    default:
        gameGrid = new(10, 10, 10);
        break;
}*/
