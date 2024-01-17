using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMinesweeper
{
	public class Menu
	{
		public struct Element
		{
			public string text;
			public ConsoleColor foreground;
		}
		ConsoleBuddy Screen;

		//logic vars
		private int _elementCursor;
		private int elementCursor
		{
			get { return _elementCursor; }
			set {
				//printing old and new highlights, correcting position
				//overwrite old
				Screen.RawPrint(new ConsoleWorker.ConsoleElement
					{
						text = Options[_elementCursor].text,
						foreground = Options[elementCursor].foreground,
						background = DefaultBackground,
						coordinates = (TopLeftCoords.x, TopLeftCoords.y + _elementCursor),
					}
				);

                //cursor corrections
                if (value == Options.Length)
                {
                    _elementCursor = 0;
                }
                else if (value == -1)
                {
                    _elementCursor = Options.Length - 1;
                }
                else
                {
                    _elementCursor = value;
                }

                //rewrite new
                Screen.RawPrint(new ConsoleWorker.ConsoleElement
					{
					    text = Options[_elementCursor].text,
					    foreground = Options[elementCursor].foreground,
					    background = CursorBackground,
					    coordinates = (TopLeftCoords.x, TopLeftCoords.y + _elementCursor),
					}
                );

            }
		} //unfinished
        private bool ElementChosen = false;
        //

        //data vars
        private Element[] Options;
		private readonly ConsoleColor CursorBackground;
		private readonly ConsoleColor DefaultBackground;
		private readonly ConsoleColor DefaultForeground;
		private readonly (int x, int y) TopLeftCoords;
		private readonly bool PrettyPrint;
		private readonly string Title;
		private readonly int MenuWidth;
		//

		public Menu(string title, bool prettyPrint, Element[] options, (int x, int y) topLeftCoords, ConsoleColor foreground, ConsoleColor background, ConsoleColor cursorBackground)
		{
			this.Options = options;
			Screen = new ConsoleBuddy(foreground, background);
			this.DefaultForeground = foreground;
			this.DefaultBackground = background;
			this.CursorBackground = cursorBackground;
			this.TopLeftCoords = topLeftCoords;
			this.PrettyPrint = prettyPrint;
			this.Title = title;
			_elementCursor = 0;
			MenuWidth = LongestElement();
			PadElements();
			PrintInitialMenu();
		}
		private void PadElements()
		{
			for (int x = 0; x< Options.Length;x++)
			{
				Options[x].text = Options[x].text.PadRight(MenuWidth);

            }
		}
		private void PrintInitialMenu()
		{
			elementCursor = 0;
			for (int y = 1; y < Options.Length; y++)
			{
                Screen.RawPrint(new ConsoleWorker.ConsoleElement
				{
					text = Options[y].text,
                    foreground = Options[y].foreground,
                    background = DefaultBackground,
                    coordinates = (TopLeftCoords.x, TopLeftCoords.y + y),
                }
                );
            }
		}
		private int LongestElement()
		{
			int length = Title.Length;
			foreach (Element element in Options)
			{
				if (element.text.Length > length) { length = element.text.Length; }
			}
			return length;
		}
		public int OperateMenu()
		{
			while (!ElementChosen)
			{
				ConsoleKey userInput = Console.ReadKey(true).Key;
				switch (userInput)
				{
					case ConsoleKey.UpArrow:
						elementCursor -= 1;
						break;
                    case ConsoleKey.DownArrow:
                        elementCursor += 1;
                        break;
					case ConsoleKey.Enter:
						ElementChosen = true; 
						break;
                }
			}
			return elementCursor;
		}
	}
}
