using System;
using Microsoft.Xna.Framework;
using Comgame.GameObject;
using Microsoft.Xna.Framework.Input;

namespace Comgame;

class Singleton
{
	private static Singleton instance;

	public const string WINDOWTITLE = "Shoot that asteroid like his those balls!";

	public const string SCOREFILE = "./Content/highscore.txt";

	public const string BESTTIMEFILE = "./Content/besttime.txt";
	public const int TILESIZE = 48;
	public const int GAMEWIDTH = 8;
	public const int GAMEHEIGHT = 10;
	public const int SCOREWIDTH = 5;
	public const int LAUNCHERHEIGHT = 2;
	public const int SCREENWIDTH = GAMEWIDTH + SCOREWIDTH;
	public const int SCREENHEIGHT = GAMEHEIGHT + LAUNCHERHEIGHT;

	public const int INITIALROWS = 4;
	public const double DROP_INTERVAL = 5.0;

	public int _score = 0;
	public int _highScore = 0;

	public TimeOnly _bestTime = new TimeOnly(0, 0, 0);
	public MovingBubble CurrentBubble;

	public MovingBubble NextBubble;

	public Bubble[,] GameBoard;

	public KeyboardState PreviousKey, CurrentKey;

	public Random Random = new Random();

	private Singleton()
	{

	}

	public static Singleton Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Singleton();
			}
			return instance;
		}
	}

	public static void rendergameboard()
	{
		for (int y = 0; y < GAMEHEIGHT; y++)
        {
            int bubbleCount = GAMEWIDTH - (y % 2);
            for (int x = 0; x < bubbleCount; x++)
            {
				var cur = Instance.GameBoard[x , y];
				if (cur != null){
					float offsetX = (y % 2 == 0) ? 0 : TILESIZE / 2;
                	float offsetY = y * TILESIZE * 0.866f; // Reduce vertical gap for hexagonal layout (0.866f = sqrt(3)/2)
					cur.Position = new Vector2(x * TILESIZE + offsetX, offsetY);
				}
            }
        }
	}

	public static void printgameboard(){
		// Print GameBoard structure to console
        Console.WriteLine("GameBoard Initialization:");
        for (int y = 0; y < GAMEHEIGHT; y++)
        {
            string row = "";
            for (int x = 0; x < GAMEWIDTH; x++)
            {
                row += Instance.GameBoard[x, y] != null ? "O " : ". ";
            }
            Console.WriteLine(row);
        }
	}
	
}