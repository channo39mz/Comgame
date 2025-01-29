using System;
using Comgame.GameObject;
using Microsoft.Xna.Framework.Input;

namespace Comgame;

class Singleton
{
	private static Singleton instance;

	public const string WINDOWTITLE = "Bubble Shooter";
	public const int TILESIZE = 48;
	public const int GAMEWIDTH = 8;
	public const int GAMEHEIGHT = 10;
	public const int SCOREWIDTH = 5;
	public const int LAUNCHERHEIGHT = 2;
	public const int SCREENWIDTH = GAMEWIDTH + SCOREWIDTH;
	public const int SCREENHEIGHT = GAMEHEIGHT + LAUNCHERHEIGHT;

	public const int INITIALROWS = 4;
	public const double DROP_INTERVAL = 5.0;

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
}