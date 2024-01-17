using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleMinesweeper
{
    internal class GameTimer
    {
        private ConsoleBuddy Screen;

        private readonly (int x, int y) coordinates;

        private Stopwatch GameTime;
        private Thread TimerThread;
        private CancellationTokenSource TimerThreadController = new CancellationTokenSource();

        public GameTimer((int x,int y)coord, ConsoleColor foreground, ConsoleColor background)
        {
            coordinates = coord;
            Screen = new ConsoleBuddy(foreground,background);

            GameTime = new();
            GameTime.Start();
            TimerThread = new Thread(() => QueueTimerPrint());
            TimerThread.Start();
        }
        public void DisposeTimer()
        {
            TimerThreadController.Cancel();
            TimerThreadController.Dispose();
            TimerThread = null;
            GameTime.Stop();
            //GameTime = null;
            

        }
        private void QueueTimerPrint()
        {
            while (!TimerThreadController.IsCancellationRequested)
            {
                Thread.Sleep(100);
                string time = String.Format("{0:0.0}", GameTime.Elapsed.TotalSeconds);
                if (time.Length > 6)
                {
                    time = time.Substring(0, 6);
                }
                Screen.DefaultPrintAtCoords(time, coordinates);
            }
        }

    }
    internal class Minesweeper
    {
        private readonly Dictionary<int, ConsoleColor> valueConsoleColourPair = new()
            {
                { 0, ConsoleColor.Black},
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
        ConsoleBuddy Screen;
        private struct CellStructure
        {
            public int value;
            public bool isFlagged;
            public bool Hidden;
        }

        private CellStructure[,] board;
        private (int x, int y) _playerCursor;
        private (int x, int y) playerCursor
        {
            get { return _playerCursor; }
            set
            {
                //correct values
                int x = value.x;
                if (x < 1) { x = 1; }
                else if (x > boardWidth) { x = boardWidth; }
                int y = value.y;
                if (y < 1) { y = 1; }
                else if (y > boardHeight) { y = boardHeight; }

                //check if cursor position changed after correction
                if (_playerCursor != (x, y))
                {
                    Screen.RawPrint( //remove old highlight
                        new ConsoleWorker.ConsoleElement()
                        {
                            text = ValueToChar(_playerCursor),
                            foreground = ValueToColor(_playerCursor),
                            background = DefaultBackground,
                            coordinates = _playerCursor
                        }
                    );
                    Screen.RawPrint( //add new highlight
                        new ConsoleWorker.ConsoleElement()
                        {
                            text = ValueToChar((x,y)),
                            foreground = ValueToColor((x,y)),
                            background = CursorBackgroundColor,
                            coordinates = (x, y)
                        }
                    );
                    _playerCursor = (x, y);
                }

            }
        }

        //init values
        private readonly ConsoleColor DefaultForeground;
        private readonly ConsoleColor DefaultBackground;
        private readonly ConsoleColor CursorBackgroundColor;
        private readonly int boardWidth;
        private readonly int boardHeight;
        private readonly int bombsAmount;
        //

        //tracker values
        private int amountRevealed;
        private int flagsLeft;
        GameTimer GameTimeElapsed;
        public bool GameState;
        public bool Win = false;
        private bool FirstReveal = true;
        //
        public Minesweeper(int boardWidth, int boardHeight, int bombsAmount, ConsoleColor cursorBackgroundColor, ConsoleColor defaultForeground, ConsoleColor defaultBackground)
        {
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            this.bombsAmount = bombsAmount;
            this.CursorBackgroundColor = cursorBackgroundColor;
            this.DefaultForeground = defaultForeground;
            this.DefaultBackground = defaultBackground;

            _playerCursor = (boardWidth / 2, boardHeight / 2);
            flagsLeft = bombsAmount;

            board = new CellStructure[boardWidth + 2, boardHeight + 2];
            Screen = new ConsoleBuddy(DefaultForeground,DefaultBackground);

            PrintInitialBoard();
            CreateNewBoard();

            GameState = true;
        }
        //game init
        private void CreateNewBoard()
        {
            //initialising empty space and boundaries
            for (int y = 1; y <= boardHeight; y++)
            {
                for (int x = 1; x <= boardWidth; x++)
                {

                    board[x, y].value = 0;
                    board[x, y].isFlagged = false;
                    board[x, y].Hidden = true;
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

            //randomly placing bombs
            PlaceBombs();

            //put the numbers on 
            PopulateBoardWithNumbers();
        }
        private void PlaceBombs()
        {
            Random rnd = new();

            int bombsPlaced = 0;
            while (bombsPlaced < bombsAmount)
            {
                int x = rnd.Next(boardWidth) + 1;
                int y = rnd.Next(boardHeight) + 1;
                if (board[x, y].value != 9)
                {
                    board[x, y].value = 9;
                    bombsPlaced++;
                }
            }
        }
        private void PopulateBoardWithNumbers()
        {
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
                        board[x, y].value = count; //CountValueAroundPoint(x, y, 9);
                    }
                }
            }
        }
        //

        //game logic
        public void GameTick()
        {
            ConsoleKey keyInput = Console.ReadKey(true).Key;
            switch (keyInput)
            {
                case ConsoleKey.UpArrow:
                    playerCursor = (playerCursor.x, playerCursor.y - 1);
                    break;
                case ConsoleKey.DownArrow:
                    playerCursor = (playerCursor.x, playerCursor.y + 1);
                    break;
                case ConsoleKey.LeftArrow:
                    playerCursor = (playerCursor.x - 1, playerCursor.y);
                    break;
                case ConsoleKey.RightArrow:
                    playerCursor = (playerCursor.x + 1, playerCursor.y);
                    break;
                case ConsoleKey.F:
                    FlipFlagState(playerCursor);
                    break;
                case ConsoleKey.Enter:
                    TryToReveal(playerCursor);
                    Screen.RawPrint(new ConsoleWorker.ConsoleElement
                    {
                        text = ValueToChar(playerCursor),
                        coordinates = playerCursor,
                        foreground = ValueToColor(playerCursor),
                        background = CursorBackgroundColor
                    });
                    break;
            }
            //UpdateWrittenAmountRevealed();
        }
        
        private void FlipFlagState((int x, int y)coords)
        {
            if (board[coords.x, coords.y].isFlagged) { board[coords.x, coords.y].isFlagged = false; flagsLeft++; }
            else if (board[coords.x, coords.y].Hidden && flagsLeft > 0 ) { board[coords.x, coords.y].isFlagged = true; flagsLeft--; }

            UpdateWrittenFlagAmount();

            Screen.RawPrint(new ConsoleWorker.ConsoleElement //write flag on board
            {
                text = ValueToChar(playerCursor),
                coordinates = playerCursor,
                foreground = ValueToColor(playerCursor),
                background = CursorBackgroundColor
            });
        }
        private void TryToReveal((int x, int y)coords)
        {
            if (FirstReveal)
            {
                if (board[coords.x, coords.y].value != 0)
                {
                    bool validBoard = false;
                    while (!validBoard)
                    {
                        //re-empty board
                        for (int y = 1; y <= boardHeight; y++)
                        {
                            for (int x = 1; x <= boardWidth; x++)
                            {
                                board[x, y].value = 0;
                            }

                        }
                        //place new bombs and check
                        PlaceBombs();
                        PopulateBoardWithNumbers();
                        validBoard = board[coords.x, coords.y].value == 0;
                    }
                }
                FirstReveal = false;
                RevealAroundZero(coords);
                GameTimeElapsed = new GameTimer((boardWidth + 2,5),DefaultForeground, DefaultBackground);
            }
            else
            {
                if (!board[coords.x, coords.y].isFlagged) {
                    if (board[coords.x, coords.y].Hidden)
                    {
                        if (board[coords.x, coords.y].value == 0) { RevealAroundZero(coords); }
                        else
                        {
                            board[coords.x, coords.y].Hidden = false;
                            amountRevealed++;
                            if (board[coords.x, coords.y].value == 9)
                            {
                                GameState = false;
                            }
                            else
                            {
                                Screen.RawPrint(
                                    new ConsoleWorker.ConsoleElement
                                    {
                                        text = ValueToChar(coords),
                                        coordinates = coords,
                                        foreground = ValueToColor(coords),
                                        background = CursorBackgroundColor,
                                    }
                                );
                            }
                        }
                    }
                    else if (!board[coords.x, coords.y].Hidden)
                    {
                        if (CountFlags(coords) == board[coords.x,coords.y].value)
                        {
                            RevealAroundPoint(coords);
                        }
                    }
                }
            }
            if (amountRevealed == (boardHeight*boardWidth) - bombsAmount)
            {
                Win = true;
                GameState = false;
            }
        }
        private void RevealAroundPoint((int x, int y) coords)
        {
            for (int y = coords.y - 1; y <= coords.y + 1; y++)
            {
                for (int x = coords.x - 1; x <= coords.x + 1; x++)
                {
                    if (!board[x, y].isFlagged && board[x,y].Hidden && board[x, y].value != 10)
                    {
                        if (board[x, y].value == 0) { RevealAroundZero((x, y)); }
                        else
                        {
                            board[x, y].Hidden = false;
                            amountRevealed++;
                            if (board[x, y].value == 9)
                            {
                                GameState = false;
                            }
                            else
                            {
                                Screen.RawPrint(
                                    new ConsoleWorker.ConsoleElement
                                    {
                                        text = ValueToChar((x,y)),
                                        coordinates = (x,y),
                                        foreground = ValueToColor((x,y)),
                                        background = DefaultBackground,
                                    }
                                );
                            }
                        }
                    }
                }
            }
        }
        private int CountFlags((int x, int y) coords)
        {
            int count = 0;
            for (int y = coords.y - 1; y <= coords.y + 1; y++)
            {
                for (int x = coords.x - 1; x <= coords.x + 1; x++)
                {
                    if (board[x, y].isFlagged)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        private void RevealAroundZero((int x, int y)coords)
        {
                for (int y = coords.y - 1; y <= coords.y + 1; y++)
                {
                    for (int x = coords.x - 1; x <= coords.x + 1; x++)
                    {
                        if (board[x, y].value != 10)
                        {
                            if (board[x, y].value == 0 && board[x, y].Hidden)
                            {
                                board[x, y].Hidden = false;
                                amountRevealed++;
                                RevealAroundZero((x, y));
                            }
                            if (board[x, y].Hidden)
                            {
                                board[x, y].Hidden = false;
                                amountRevealed++;
                            }
                            if (board[x, y].isFlagged) { board[x, y].isFlagged = false; flagsLeft++; UpdateWrittenFlagAmount(); }

                            Screen.RawPrint(
                                new ConsoleWorker.ConsoleElement
                                {
                                    text = ValueToChar((x, y)),
                                    coordinates = (x, y),
                                    foreground = ValueToColor((x, y)),
                                    background = DefaultBackground,
                                }
                            );
                        }
                    }

                }
        }
        //

        //game end funcs
        private void PrintAllBombs()
        {
            for (int y = 1; y <= boardHeight; y++)
            {
                for (int x = 1; x <= boardWidth; x++)
                {
                    if (board[x, y].value == 9)
                    {
                        Screen.RawPrint(new ConsoleWorker.ConsoleElement
                        {
                            text = "Ø",
                            coordinates = (x,y),
                            foreground = ValueToColor((x,y)),
                            background = CursorBackgroundColor,
                        });
                    }
                }
            }
        }
        public void GameOverSequence()
        {
            PrintAllBombs();
            GameTimeElapsed.DisposeTimer();
            
            //dispose of everything
        }
        //

        //print functions
        private void UpdateWrittenAmountRevealed()
        {
            Screen.DefaultPrintAtCoords(Convert.ToString(amountRevealed), (boardWidth + 2, 9));
        }
        private void UpdateWrittenFlagAmount()
        {
            Screen.DefaultPrintAtCoords(String.Format("{0,-6}", flagsLeft), (boardWidth + 2, 2)); //update written flag count
        }
        private void PrintStatsBoardSide()
        {
            int boxWidth = 6;
            Screen.DefaultPrintAtCoords(("╦" + new String('═', boxWidth) + "╗"), (boardWidth + 1, 0));
            Screen.DefaultPrintAtCoords(("║" + "Flags" + new string(' ', boxWidth - 5) + "║"), (boardWidth + 1, 1));
            Screen.DefaultPrintAtCoords(("║" + flagsLeft + new string(' ', boxWidth - Convert.ToString(flagsLeft).Length) + "║"), (boardWidth + 1, 2));
            Screen.DefaultPrintAtCoords(("╠" + new String('═', boxWidth) + "╣"), (boardWidth + 1, 3));
            Screen.DefaultPrintAtCoords(("║" + "Timer" + new string(' ', boxWidth - 5) + "║"), (boardWidth + 1, 4));
            Screen.DefaultPrintAtCoords(("║" + "0.0" + new string(' ', boxWidth - 3) + "║"), (boardWidth + 1, 5));
            Screen.DefaultPrintAtCoords(("╠" + new String('═', boxWidth) + "╝"), (boardWidth + 1, 6));
        }
        private void PrintInitialBoard()
        {
            Screen.DefaultPrintAtCoords(("╔" + new String('═', boardWidth) + "╗"),(0,0));
            for (int y = 1; y <= boardHeight; y++)
            {
                Screen.DefaultPrintAtCoords(("║" + new string('·', boardWidth) + "║"),(0,y));
            }
            Screen.DefaultPrintAtCoords(("╚" + new String('═', boardWidth) + "╝"),(0,boardHeight+1));
            Screen.RawPrint(new ConsoleWorker.ConsoleElement
            {
                text = ValueToChar(playerCursor),
                coordinates = playerCursor,
                foreground = ValueToColor(playerCursor),
                background = CursorBackgroundColor
            });
            PrintStatsBoardSide();
        }
        private string ValueToChar((int x, int y) coords)
        {
            if (board[coords.x, coords.y].isFlagged == true)
            {
                return "¶";
            }
            else if (board[coords.x, coords.y].Hidden == true)
            {
                return "·";
            }
            int val = board[coords.x, coords.y].value;
            if (val == 0) { return " "; }
            else
            {
                return Convert.ToString(val);
            }
        }
        private ConsoleColor ValueToColor((int x, int y) coords)
        {
            if (board[coords.x, coords.y].isFlagged == true)
            {
                return ConsoleColor.DarkRed;
            }
            else if (board[coords.x, coords.y].Hidden == true)
            {
                return DefaultForeground;
            }
            else
            {
                return valueConsoleColourPair[board[coords.x, coords.y].value];
            }
        }
        public void DebugPrintBoard()
        {
            string s = String.Empty;
            for (int y = 0; y <= boardHeight + 1; y++)
            {
                for (int x = 0; x <= boardWidth +1; x++)
                {
                    int val; 
                    if(board[x, y].Hidden) { val = board[x, y].value; } else { val = -1 * board[x, y].value; }
                    if (val == -10) { val = 7; }
                    if (playerCursor == (x, y)) { val = 8; }
                    s += Convert.ToString(val);
                }
                s += "\n";

            }
            Screen.DefaultPrintAtCoords(s, (0, boardHeight + 5));
        }
        //
    }
}
