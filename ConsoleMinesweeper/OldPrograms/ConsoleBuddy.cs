using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ConsoleMinesweeper
{

    //public class fartfart 
    /*
    public class ConsoleWorker
    {
        private Thread Turtle;
        private CancellationTokenSource WorkerToken = new CancellationTokenSource();
        private ConcurrentQueue<Action> PrintQueue = new ConcurrentQueue<Action>();

        public ConsoleWorker()
        {
            Turtle = new Thread(() => ThreadRoutine());
            Turtle.Start();
        }
        public struct ConsoleElement
        {
            public string text;
            public ConsoleColor? foreground;
            public ConsoleColor? background;
            public (int x, int y) coordinates;
        }
        public void UnsafeDisposeWorker()
        {
            PrintQueue.Clear();
            WorkerToken.Cancel();
            Turtle = new Thread(() => { return; });
        }
        public void QueueRawPrint(ConsoleElement item)
        {
            PrintQueue.Enqueue(() => RawPrintOnConsole(item));
        }
        private void ThreadRoutine()
        {
            while (!WorkerToken.IsCancellationRequested)
            {
                Thread.Sleep(10);
                Action PrintTask;
                while (PrintQueue.TryDequeue(out PrintTask))
                {
                    try
                    {

                        PrintTask();
                        Debug.WriteLine("printing a task");
                    }
                    catch
                    {
                        Debug.WriteLine("Something went wrong lmaooo");
                    }
                }
                if (PrintQueue.Count == 0)
                {
                    Debug.WriteLine("Queue is empty");
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
    /*
    internal class ProgramTwo
    {
        static void Main(string[] args)
        {
            ConsoleWorker pen = new();
            ConsoleWorker.ConsoleElement helloText = new ConsoleWorker.ConsoleElement()
            {
                text = "Hello world!",
                foreground = ConsoleColor.Red,
                background = ConsoleColor.Gray,
                coordinates = (2, 0),
            };

            for (int x = 0; x < 100; x++)
            {
                helloText.coordinates.y = x;

                pen.QueueRawPrint(helloText);
                pen.ResumeWorker();
            }
            pen.PauseWorker();
            for (int x = 0; x < 100; x++)
            {
                helloText.coordinates.y = +x;

                pen.QueueRawPrint(helloText);

            }

            Thread.Sleep(5000);
            pen.ResumeWorker();
            pen.UnsafeDisposeWorker();
            //for (int x = 0; x < 200;x++) { pen.ResumeWorker(); }
            //pen.QueueRawPrint(helloText);
        }

    }
    */


}