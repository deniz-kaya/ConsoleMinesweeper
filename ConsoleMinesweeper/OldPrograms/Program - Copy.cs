using System.Diagnostics;
using System.Runtime.InteropServices;

/* templates
for (int y = 1; y <= boardHeight; y++)
{
for (int x = 1; x <= boardWidth; x++)
{
//logic
}
}

*/


/// TODO
/// - finish menu class --- donr 
/// - add a threaded timer to the screen
/// 
/// IMPROVEMENTS
/// - Queue system for drawing to the console
/// - subroutine for: revealing individual cells and adding to revealed count
///
/// 
/// KNOWN BUGS
/// - reveal count is messed up
/// - flag count doesnt clear the previous value
/// - timer sometimes bugs: more testing needed as the new change to using only seconds might have fixed it.
/// 

namespace ConsoleMinesweeper
{
    
    //public class 
    public class ConsoleDraw
    {
        private (int x, int y) referencePoint;
        public (int x, int y) ConsoleCursor
        {
            get { return Console.GetCursorPosition(); }
            set { Console.SetCursorPosition(value.x, value.y); }
        }
        public void ChangeReferencePoint((int x, int y) newPoint)
        {
            referencePoint = newPoint;
        }
        public ConsoleDraw(ConsoleColor foreground, ConsoleColor background, (int x, int y) reference)
        {
            Console.CursorVisible = false;

            this.referencePoint = reference;

            this.DefaultForeground = foreground;
            this.DefaultBackground = background;

            this.Foreground = foreground;
            this.Background = background;
        }
        public readonly ConsoleColor DefaultForeground;
        public readonly ConsoleColor DefaultBackground;
        public ConsoleColor Foreground
        {
            get { return Foreground; }
            set { Console.ForegroundColor = value; }
        }
        public ConsoleColor Background
        {
            get { return Background; }
            set { Console.BackgroundColor = value; }
        }
        public void DefaultColors()
        {
            Background = DefaultBackground;
            Foreground = DefaultForeground;
        }
        public void WriteLine(string value)
        {
            Console.Write(value);
            DefaultColors();
            NewLine();
        }
        public void Write(string value)
        {
            Console.Write(value);
            DefaultColors();
        }
        public void NewLine()
        {
            ConsoleCursor = (referencePoint.x, (ConsoleCursor.y + 1));
        }
    }
    public class Menu
    {
        private int elementCursor = 0;
        private readonly ConsoleColor HighlightColor;
        private readonly (int x, int y) OriginalCursorStart;
        private (int x, int y) CursorStart;
        private readonly bool prettyPrint;
        private readonly int elementCount;
        private readonly int menuWidth;
        private readonly string title;
        public Menu(bool prettyPrint,
                    ConsoleColor foreground,
                    ConsoleColor background,
                    ConsoleColor highlightColor,
                    Element[] listElements,
                    string? title,
                    (int x, int y) menuTopLeft)
        {
            this.prettyPrint = prettyPrint;
            if (title == null) { this.title = String.Empty; } else { this.title = title; }
            Cursor = new(foreground, background, menuTopLeft);
            HighlightColor = highlightColor;
            CursorStart = menuTopLeft;
            OriginalCursorStart = menuTopLeft;
            elements = listElements;
            elementCount = elements.Length;
            menuWidth = PadElementNames();
        }
        public struct Element
        {
            public string name;
            public ConsoleColor foreground;
        }
        Element[] elements;
        ConsoleDraw Cursor;
        private int PadElementNames()
        {
            //gets longest length
            int longest = title.Length;
            foreach (Element item in elements)
            {
                if (item.name.Length > longest) { longest = item.name.Length; }
            }

            //
            for (int x = 0; x < elements.Length; x++)
            {
                int currentLength = elements[x].name.Length;
                elements[x].name = elements[x].name + new String(' ', (longest - currentLength));
            }
            return longest;
            //
        }
        private void PrintMenu()
        {
            Cursor.ChangeReferencePoint(OriginalCursorStart);
            CursorStart = OriginalCursorStart;
            Cursor.ConsoleCursor = CursorStart;
            Cursor.DefaultColors();

            if (prettyPrint)
            {
                if (title != String.Empty)
                {
                    Cursor.WriteLine("╔" + title + new string('═', menuWidth - title.Length) + "╗");
                }
                else
                {
                    Cursor.WriteLine("╔" + new string('═', menuWidth) + "╗");
                }
                for (int x = 1; x <= elementCount; x++)
                {
                    Cursor.WriteLine(new string('║', menuWidth + 2));
                }
                Cursor.Write("╚" + new string('═', menuWidth) + "╝");
                CursorStart = (CursorStart.x + 1, CursorStart.y + 1);
                Cursor.ChangeReferencePoint(CursorStart);
            }
            else if (title != String.Empty)
            {
                Cursor.WriteLine(title);
                CursorStart = (CursorStart.x, CursorStart.y + 1);
            }
            Cursor.ConsoleCursor = CursorStart;


            Cursor.Foreground = elements[0].foreground;
            Cursor.Background = HighlightColor;

            Cursor.WriteLine(elements[0].name);

            Cursor.DefaultColors();
            for (int x = 1; x < elementCount; x++)
            {
                Cursor.Foreground = elements[x].foreground;
                Cursor.WriteLine(elements[x].name);
            }
        }
        private void DrawHighlight(int old, int n) //some redundant pieces of code, could be optimised
        {
            Cursor.ConsoleCursor = (CursorStart.x, old + CursorStart.y);
            Cursor.Background = Cursor.DefaultBackground;
            Cursor.Foreground = elements[old].foreground;
            Cursor.Write(elements[old].name);

            Cursor.ConsoleCursor = (CursorStart.x, n + CursorStart.y);
            Cursor.Background = HighlightColor;
            Cursor.Foreground = elements[n].foreground;
            Cursor.Write(elements[n].name);
        }
        public int OperateMenu()
        {

            PrintMenu();
            elementCursor = 0;
            while (true)
            {
                ConsoleKey input = Console.ReadKey().Key;

                switch (input)
                {
                    case ConsoleKey.UpArrow:
                        DrawHighlight(elementCursor, (elementCursor + (elementCount - 1)) % elementCount);
                        elementCursor = (elementCursor + (elementCount - 1)) % elementCount;
                        Debug.WriteLine(elementCursor);

                        break;
                    case ConsoleKey.DownArrow:
                        DrawHighlight(elementCursor, (elementCursor + (elementCount + 1)) % elementCount);
                        elementCursor = (elementCursor + (elementCount + 1)) % elementCount;
                        Debug.WriteLine(elementCursor);
                        break;
                    case ConsoleKey.Enter:
                        return elementCursor;
                }

            }
        }
    }

    // overall class is well-built, but a lot of functions can be minimized/combined e.g. initializecleanboard and initialize bombs can be identical just have bombs = 0?
    public class Game
    {
        private static object _consoleCursorLock = new();
        const ConsoleColor defaultColor = ConsoleColor.Black;
        readonly Dictionary<int, ConsoleColor> valueConsoleColourPair = new()
        {
            { 0, defaultColor},
            { 1, ConsoleColor.Blue},
            { 2, ConsoleColor.Green},
            { 3, ConsoleColor.Red},
            { 4, ConsoleColor.DarkBlue },
            { 5, ConsoleColor.DarkRed },
            { 6, ConsoleColor.Cyan},
            { 7, ConsoleColor.DarkMagenta},
            { 8, ConsoleColor.DarkGray},
            { 9, ConsoleColor.Black},
            { 10,ConsoleColor.Black}
        };
        public Game(int height, int width, int bombs)
        {
            Console.CursorVisible = false;
            gameState = true;
            boardHeight = height;    
            boardWidth = width;
            amountBombs =  bombs;
            amountHidden = boardHeight * boardWidth;
            playerCursor = (boardHeight / 2, boardHeight / 2);
            CreateNewBoard();
        }
        struct CellStructure
        {
            //potentially have more verbose bomb checking instead of value = 9, even though value works it might have issues sometimes
            public int value;
            public bool isHidden;
            public bool isFlagged;

        }
        public void StartTimer()
        {
            gameTime = new();
            gameTime.Start();
            timerPrinter = new Thread(() => updateTimer());
            timerPrinter.Start();
        }
        private Stopwatch gameTime;
        private Thread timerPrinter;
        private ManualResetEvent TimerPrinterController = new ManualResetEvent(true);
        private bool timerpause = false;
        //Queue<ConsoleDraw> drawQueue = new Queue<ConsoleDraw>();
        
        private void updateTimer()
        {
            //drawQueue.Enqueue(new ConsoleDraw());
            while (true)
            {
                if (!timerpause)
                {
                    Thread.Sleep(20);
                    if (!timerpause)
                    {
                        TimerPrinterController.WaitOne();
                        lock (_consoleCursorLock)
                        {
                            consoleCursor = (7, boardHeight + 5);
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.Write(String.Format("{0:00.0}", gameTime.Elapsed.TotalSeconds));
                            //Console.Write(Convert.ToString(Math.Round(gameTime.Elapsed.TotalSeconds)));
                            //PrintAt((7, boardHeight + 5), (String.Format("{0:00.0}", gameTime.Elapsed.TotalSeconds),ConsoleColor.Black),ConsoleColor.Gray);
                            consoleCursor = playerCursor;
                        }
                    }
                }
            }
        }
        
        private bool gameState;
        private bool isFirstReveal = true;
        private static (int x, int y) playerCursor;
        private static (int x, int y) consoleCursor
        {
            get { return Console.GetCursorPosition(); }
            set { Console.SetCursorPosition(value.x, value.y); }
        }

        CellStructure[,] board; //potentially overflow edges around the board, to allow for a 3x3 reveal to be coded well
        
        public int boardWidth, boardHeight, amountBombs; 
        public int amountHidden, amountFlagged;
        private void CreateNewBoard()
        {
            //InitialiseCleanBoard();
            board = new CellStructure[boardWidth + 2, boardHeight + 2];
            for (int y = 0; y <= boardHeight + 1; y++)
            {
                for (int x = 0; x <= boardWidth + 1; x++)
                {

                    board[x, y].value = 0;
                    board[x, y].isFlagged = false;
                    board[x, y].isHidden = true;
                }

            }
            for (int x = 0; x <= boardWidth + 1; x++)
            {
                board[x, 0].value = 10;
                board[x, boardHeight + 1].value = 10;
            }
            for (int y = 0; y <= boardHeight + 1; y++)
            {
                board[0, y].value = 10;
                board[boardWidth + 1, y].value = 10;
            }
            //
            //PlaceBombs(amountBombs);
            Random rnd = new();
           
            int bombsPlaced = 0;
            while (bombsPlaced < amountBombs)
            {
                int x = rnd.Next(boardWidth) + 1;
                int y = rnd.Next(boardHeight) + 1;
                if (board[x, y].value != 9)
                {
                    board[x, y].value = 9;
                    bombsPlaced++;
                }
            }

            //
            //FillNumbersOnBoard();
            for (int y = 1; y <= boardHeight; y++)
            {
                for (int x = 1; x <= boardWidth; x++)
                {
                    if (board[x, y].value != 9)
                    {
                        int count = 0;
                        for (int i = y - 1; i <= y + 1; i++)
                        {
                            for (int j = x - 1; j <= x + 1; j++)
                            {
                                if (board[j, i].value == 9)
                                {
                                    count++;
                                }
                            }
                        }
                        if (board[x, y].value == 9) { count--; }

                        board[x, y].value = count; //CountValueAroundPoint(x, y, 9);
                    }
                }
            }

            //set cursor to the middle of the board
            
        }
        private static void PrintAt((int x, int y) printPos, (string s, ConsoleColor foreground) spotInfo, ConsoleColor background)
        {
            lock (_consoleCursorLock)
            {
                consoleCursor = printPos;
                //ConsoleColor curentForeground = Console.ForegroundColor;
                //ConsoleColor currentBackground = Console.BackgroundColor;
                Console.ForegroundColor = spotInfo.foreground;
                Console.BackgroundColor = background;
                Console.Write(spotInfo.s);
            }
            //Console.ForegroundColor = curentForeground;
            //Console.BackgroundColor = currentBackground;
        }
        public bool GameTick(ConsoleKey inputKey)
        {
            timerpause = true;
            TimerPrinterController.Reset();
            switch (inputKey)
            {
                
                case ConsoleKey.LeftArrow:
                    {
                        //left logic

                        PrintAt(playerCursor, ValueColorToPrint(playerCursor), ConsoleColor.Gray);
                        playerCursor = (playerCursor.x - 1, playerCursor.y);
                        CorrectPlayerCursor();
                        break;
                    }
                case ConsoleKey.RightArrow:
                    {
                        //right logic

                        PrintAt(playerCursor, ValueColorToPrint(playerCursor), ConsoleColor.Gray);
                        playerCursor = (playerCursor.x + 1, playerCursor.y);
                        CorrectPlayerCursor();
                        break;
                    }
                case ConsoleKey.UpArrow:
                    {
                        //up logic

                        PrintAt(playerCursor, ValueColorToPrint(playerCursor), ConsoleColor.Gray);
                        playerCursor = (playerCursor.x, playerCursor.y - 1);
                        CorrectPlayerCursor();
                        break;
                    }
                case ConsoleKey.DownArrow:
                    {
                        //down logic

                        PrintAt(playerCursor, ValueColorToPrint(playerCursor), ConsoleColor.Gray);
                        playerCursor = (playerCursor.x, playerCursor.y + 1);
                        CorrectPlayerCursor();
                        break;
                    }
                case ConsoleKey.F:
                    {
                        FlipFlagState(playerCursor);
                        PrintStats();
                        break;
                    }
                case ConsoleKey.Enter:
                    {
                        TryToReveal(playerCursor);
                        PrintStats();
                        break;
                    }
                default:
                    {

                        break;
                    }
            }

            PrintAt(playerCursor, ValueColorToPrint(playerCursor), ConsoleColor.Yellow);
            
            /*
            Debug.WriteLine($"playercursor is at {playerCursor}");
            Debug.WriteLine(gameState);
            Debug.WriteLine($"current amounthiddne: {amountHidden}");
            Debug.WriteLine(ThreadPool.ThreadCount.ToString());
            */
            PrintStats();
            
            consoleCursor = playerCursor;
            timerpause = false;
            TimerPrinterController.Set();
            return gameState;
        }
        private int CountForFlags((int x, int y)point)
        {
            int count = 0;
            for (int i = point.y - 1; i <= point.y + 1; i++)
            {
                for (int j = point.x - 1; j <= point.x + 1; j++)
                {
                    if (board[j, i].isFlagged)
                    {
                        count++;
                    }
                }
            }
            return count;

        }
        private void TryToReveal((int x, int y) point)
        {
            if (isFirstReveal == true && !board[point.x,point.y].isFlagged)
            {
                //int i =0;
                while (board[point.x, point.y].value != 0)
                {
                    CreateNewBoard(); 
                    //Debug.WriteLine($"current it: {i}");
                }
                
                isFirstReveal = false;
                TryToReveal(point);
                RevealAroundPointZero(point);
            } 
            else if (!board[point.x, point.y].isFlagged && board[point.x,point.y].isHidden)
            {
                board[point.x, point.y].isHidden = false;
                if (board[point.x,point.y].value == 9)
                {
                    gameState = false;
                    return;
                }
                amountHidden--;
                if (board[point.x, point.y].value == 0)
                {
                    RevealAroundPointZero(point);
                }
            }
            else if (!board[point.x,point.y].isHidden)
            {
                if (CountForFlags(point) == board[point.x,point.y].value)
                {
                    RevealAroundPoint(point);

                }
            }
        }

        //private int CountBorderRevealed()
        //{
        //    int count = 0;
        //    for (int  = 0, x < boardHeight +2)
        //    return count;
        //}
        private void CorrectPlayerCursor()
        {
            if (playerCursor.x > boardWidth) { playerCursor = (boardWidth, playerCursor.y); }
            else if (playerCursor.x < 1) { playerCursor = (1, playerCursor.y); }
            if (playerCursor.y > boardHeight) { playerCursor = (playerCursor.x, boardHeight); }
            else if (playerCursor.y < 1) { playerCursor = (playerCursor.x, 1); }
        }
        private (string,ConsoleColor) ValueColorToPrint((int x, int y) position)
        {
            string value;
            ConsoleColor color = ConsoleColor.Black;
            int x = position.x;
            int y = position.y;
            int currentVal = board[x, y].value;
            
            if (board[x, y].isFlagged) 
            { 
                value = "¶"; color = ConsoleColor.DarkRed; 
            }
            else if (position.x > boardWidth || position.x < 1)
            {
                if (position.y < 1 && position.x > boardWidth)
                {
                    value = "╗";
                }
                else if (position.y < 1 && position.x <1)
                {
                    value = "╔";
                }
                else if (position.y > boardHeight && position.x > boardWidth)
                {
                    value = "╝";
                }
                else if (position.y > boardHeight && position.x < 1)
                {
                    value = "╚";
                }
                else
                {
                    value = "║";
                }
                color = ConsoleColor.Black;
            }
            else if (position.y > boardHeight || position.y < 1)
            {
                value = "═";
                color = ConsoleColor.Black;
            }
            else if (!board[x, y].isHidden)
            {
                //if (playerCursor.x == x && playerCursor.y == y) { Console.BackgroundColor = ConsoleColor.Yellow; }
                
                if (currentVal == 9) { value = "Ø"; }
                else if (currentVal > 0)
                {
                    value = currentVal.ToString();
                    color = valueConsoleColourPair[currentVal];
                }
                else
                {
                    value = " ";
                }
            }
            else
            {
                value = "·";
            }
            return (value,color);
        }
        private void FlipFlagState((int x , int y) point)
        {
            if (board[point.x, point.y].isHidden)
            {
                board[point.x, point.y].isFlagged = !board[point.x, point.y].isFlagged;
                if (board[point.x, point.y].isFlagged) { amountFlagged++; } else { amountFlagged--; }
            }
        }
        private void RevealAroundPoint((int x, int y) point)
        {
            for (int i = point.y - 1; i <= point.y + 1; i++)
            {
                for (int j = point.x - 1; j <= point.x + 1; j++)
                {
                    if (!board[j,i].isFlagged && board[j,i].isHidden)
                    {
                        if (board[j, i].value == 0) { RevealAroundPointZero((j, i)); }
                        else if (board[j,i].value == 9)
                        {
                            gameState = false;
                            break;
                        }
                        else if (!(board[j,i].value == 10))
                        {
                            board[j, i].isHidden = false;
                            amountHidden--;
                            PrintAt((j, i), ValueColorToPrint((j, i)), ConsoleColor.Gray);
                        }
                        
                    }
                }
            }
        }
        private void RevealAroundPointZero((int x, int y) point)
        {


            //if (board[point.x, point.y].value == 0) { board[point.x, point.y].isHidden = false; }
            //Debug.WriteLine(ThreadPool.ThreadCount.ToString());

            for (int i = point.y - 1; i <= point.y + 1; i++)
                {
                for (int j = point.x - 1; j <= point.x + 1; j++)
                {
                    if (board[j, i].value != 10)
                    {

                        if (board[j, i].value == 0 && board[j, i].isHidden)
                        {
                            Debug.WriteLine($"revealing around 0 at point {j},{i}");
                            board[j, i].isHidden = false;
                            amountHidden--;

                            RevealAroundPointZero((j, i));
                        }
                        if (board[j, i].isHidden)
                        {
                            board[j, i].isHidden = false;
                            amountHidden--;
                        }
                        if (board[j, i].isFlagged) { board[j, i].isFlagged = false; }
                        PrintAt((j, i), ValueColorToPrint((j, i)), ConsoleColor.Gray);
                    }
                }
                    
            }
                       
        }

        private void PrintStats()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Gray;
            consoleCursor = (0, boardHeight + 2);
            Console.Write($"Amount Revealed - {(boardHeight * boardWidth) - amountHidden}\nAmount flagged - {amountFlagged}\nBombs - {amountBombs}\nTime - "); //(7,boardHeight + 6)
            Console.BackgroundColor = ConsoleColor.Gray;
        }
        public void DebugPrint()
        {
            for (int y = 0; y <= boardHeight + 1; y++)
            {
                for (int x = 0; x <= boardWidth + 1; x++)
                {
                    Console.Write(board[x, y].value);
                }
                Console.WriteLine();
            }
        }
        public void GameOverSequence()
        {
            gameTime.Stop();
            timerPrinter = new Thread(() => { return; });
            TimerPrinterController.Reset();
            //timerPrinter.Join();
            ShowAllBombsOnScreen();
        }
        private void ShowAllBombsOnScreen()
        {
            for (int y = 1; y <= boardHeight; y++)
            {
                for (int x = 1; x <= boardWidth; x++)
                {
                    if (board[x,y].value == 9)
                    {
                        consoleCursor = (x, y);
                        Console.ForegroundColor = ValueColorToPrint((x, y)).Item2;
                        Console.Write("Ø");
                    }
                }
            }
        }
        public void PrintStartingBoard()
        { //╔ ╗ ╚ ╝ ║ ═
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.WriteLine("╔" + new String('═', boardWidth) + "╗");
            for (int y = 1; y <= boardHeight; y++)
            {
                /*
                Console.Write("║");
                for (int x = 1; x <= boardWidth; x++)
                {
                    int currentVal = board[x, y].value;

                    if (!board[x, y].isHidden)
                    {
                        //if (playerCursor.x == x && playerCursor.y == y) { Console.BackgroundColor = ConsoleColor.Yellow; }
                        if (board[x, y].isFlagged) { Console.Write("P"); }
                        else if (currentVal == 9) { Console.Write("O"); }
                        else if (currentVal > 0)
                        {
                            Console.ForegroundColor = valueConsoleColourPair[board[x, y].value];
                            Console.Write(board[x, y].value);
                            Console.ForegroundColor = defaultColor;
                        }
                        else
                        {
                            Console.Write(" ");
                        }
                    }
                    else
                    {
                        Console.Write("·");
                    }
                    Console.BackgroundColor = ConsoleColor.Gray;
                }
                Console.Write("║");
                Console.WriteLine();
                */
                Console.WriteLine("║"+ new string('·',boardWidth)+ "║");
            }
            Console.WriteLine("╚" + new String('═', boardWidth) + "╝");
            Console.BackgroundColor = ConsoleColor.Black;
            PrintStats();
        }      
    }

    //todo:
    //reveal all empty space connected
    //if cursor first reveal is not empty, keep generating new reveals
    internal class Program
    {
        static void sdMain()
        {
            Menu.Element[] difficultySelectElements = new Menu.Element[]
        {
            new Menu.Element{name = "Easy", foreground = ConsoleColor.Green},
            new Menu.Element{name = "Medium", foreground = ConsoleColor.Red},
            new Menu.Element{name = "Hard", foreground = ConsoleColor.Magenta},
            new Menu.Element{name = "Insane", foreground = ConsoleColor.DarkMagenta},
            //new Menu.Element{name = "Custom", foreground = ConsoleColor.Blue},
        };
            Menu.Element[] gameOverElements = new Menu.Element[]
            {
            new Menu.Element {name = "Yes", foreground = ConsoleColor.Green},
            new Menu.Element {name = "No", foreground = ConsoleColor.Red},
            };


            Game gameGrid;
            Menu difficulty = new(true, ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Yellow, difficultySelectElements, "Select Difficulty",(0,0));
            Menu gameOver = new(true,ConsoleColor.White,ConsoleColor.DarkGray,ConsoleColor.Blue,gameOverElements,"Game Over! Play again?",(0,0));
            bool playAgain = true;
            while (playAgain)
            {

                int diff = difficulty.OperateMenu();
                switch (diff)
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
                }
                //gameGrid = new(30, 50, 250);
                Console.Clear();
                gameGrid.PrintStartingBoard();
                gameGrid.StartTimer();
                bool gameState = true;
                
                while (gameState)
                {
                    ConsoleKey keyInput = Console.ReadKey().Key;
                    
                    gameState = gameGrid.GameTick(keyInput);
                    if (gameState == false)
                    {
                        gameGrid.GameOverSequence();

                        Thread.Sleep(2000);

                        if (gameOver.OperateMenu() == 1)
                        {
                            playAgain = false;
                            gameGrid = new Game(10,10,1);
                        }
                        Console.Clear();
                    }
                }
            }
            
            
        }
    }
}