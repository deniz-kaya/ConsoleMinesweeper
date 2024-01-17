using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMinesweeper
{
    public class ConsoleBuddy
    {
        static ConsoleWorker worker = new ConsoleWorker(); //static means the same instance is shared throughout classes :D
        private readonly ConsoleColor DefaultForeground;
        private readonly ConsoleColor DefaultBackground;
        public static (int x, int y) ConsoleCursor
        {
            get { return Console.GetCursorPosition(); }
            set { Console.SetCursorPosition(value.x, value.y); } 
        }
        public ConsoleBuddy(ConsoleColor defaultForeground, ConsoleColor defaultBackground)
        {
            
            DefaultForeground = defaultForeground;
            DefaultBackground = defaultBackground;
        }
        public void NewLine()
        {
            ConsoleCursor = (ConsoleCursor.x, ConsoleCursor.y + 1);
        }
        public void DefaultPrintAtCursor(string s)
        {
            worker.QueueRawPrint(
                new ConsoleWorker.ConsoleElement 
                { 
                    text = s,
                    coordinates = ConsoleCursor,
                    background = DefaultBackground,
                    foreground = DefaultForeground
                }
            );
        }
        public void DefaultPrintAtCoords(string s, (int x, int y)coords)
        {
            worker.QueueRawPrint(
                new ConsoleWorker.ConsoleElement
                    {
                        text = s,
                        foreground = DefaultForeground,
                        background = DefaultBackground,
                        coordinates = coords
                    }
            ); 
        }
        public void PrintAtCursorWithColors(string s, (ConsoleColor foreground, ConsoleColor background) colors)
        {
            worker.QueueRawPrint(
                new ConsoleWorker.ConsoleElement
                {
                    text = s,
                    coordinates = ConsoleCursor,
                    foreground = colors.foreground, 
                    background = colors.background
                }

            );
        }
        public void CustomPrint(string s, (int x, int y) coords, (ConsoleColor foreground, ConsoleColor background) colors)
        {
            worker.QueueRawPrint(
                new ConsoleWorker.ConsoleElement
                {
                    text = s,
                    coordinates = coords,
                    background = colors.background,
                    foreground = colors.foreground
                }
            );
        }
        public void Print(string s, (int x, int y)? coords, (ConsoleColor foreground, ConsoleColor background)? nullableColors)
        {
            ConsoleWorker.ConsoleElement item = new ConsoleWorker.ConsoleElement();
            item.text = s;
            if (coords == null)
            {
                item.coordinates = ConsoleCursor;
            }
            else { item.coordinates = ((int x, int y))coords; }


            if (nullableColors == null)
            {
                item.background = DefaultBackground;
                item.foreground = DefaultForeground;
            }
            else 
            { 
                (ConsoleColor foreground, ConsoleColor background) colors = ((ConsoleColor foreground, ConsoleColor background))nullableColors;
                item.foreground = colors.foreground;
                item.background = colors.background; 
            }
            worker.QueueRawPrint(item);
        } //only recommended for ease of use, weird nullable stuff going on.
        public void RawPrint(ConsoleWorker.ConsoleElement item)
        {
            worker.QueueRawPrint(item);
        }
    }
    public class ConsoleWorker
    {
        private Thread Turtle;
        private CancellationTokenSource WorkerToken = new CancellationTokenSource();
        private ConcurrentQueue<Action> PrintQueue = new ConcurrentQueue<Action>();

        public ConsoleWorker()
        {
            Console.CursorVisible = false;
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Turtle = new Thread(() => ThreadRoutine());
            Turtle.Start();
        }
        public struct ConsoleElement
        {
            public string text;
            public ConsoleColor foreground;
            public ConsoleColor background;
            public (int x, int y) coordinates;
        }
        public void UnsafeDisposeWorker()
        {
            PrintQueue = null;
            WorkerToken.Cancel();
            WorkerToken.Dispose();
        }
        public void QueueRawPrint(ConsoleElement item)
        {
            PrintQueue.Enqueue(() => RawPrintOnConsole(item));
        }
        private void ThreadRoutine()
        {
            while (!WorkerToken.IsCancellationRequested)
            {
                Console.SetWindowSize(49, 32);
                Console.SetBufferSize(49, 32);
                Action PrintTask;
                while (PrintQueue.TryDequeue(out PrintTask))
                {
                    PrintTask();
                    /*
                    Debug.WriteLine("printing a task");
                }
                if (PrintQueue.Count == 0)
                {
                    Debug.WriteLine("Queue is empty");
                    */
                }
            }
        }
        private static void RawPrintOnConsole(ConsoleElement item)
        {
            Console.SetCursorPosition(item.coordinates.x, item.coordinates.y);
            Console.ForegroundColor = item.foreground;
            Console.BackgroundColor = item.background;
            Console.Write(item.text);
            
        }
        //not implemented:
        public void PauseWorker()
        {
            throw new NotImplementedException();
        }
        public void PrintNow(ConsoleElement item)
        {
            throw new NotImplementedException();
        }
        public void ResumeWorker()
        {
            throw new NotImplementedException();
        }
        // ends here
    }
}
