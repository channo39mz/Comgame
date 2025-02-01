using System;
using Microsoft.Xna.Framework;
using Comgame.GameObject;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

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

	public int Score = 0;
	public int HighScore = 0;
	public double GameStartTime = 0;

	public double BestTime = 0;
	public MovingBubble CurrentBubble;

	public MovingBubble NextBubble;

	public Bubble[,] GameBoard;

	public KeyboardState PreviousKey, CurrentKey;
	public SoundEffect exploded;
	public SoundEffect dropRow;

	public Random Random = new Random();

	public const int SHOTS_BEFORE_DROP = 5;
	public int ShotCounter = 0;

	public bool IsTopRowEven = true;

	public const double CEILING_DROP_INTERVAL = 5.0; // Drop every 10 seconds (adjust as needed)
	public double CeilingDropTimer = 0.0;
	public static bool IsCeilingDropping = false;
	public static event Action<Vector2> OnBubbleDestroyed;

	public int LastComboCount { get; private set; } = 0;
	public Vector2 LastComboPosition { get; private set; } = Vector2.Zero;
	public double ComboDisplayTimer { get; private set; } = 0;
	private const double ComboDisplayDuration = 1.0; // Display for 1 second


	public enum GameState
	{
		MainMenu,
		InGame,
		GameWon,
		GameLose
	}

	public GameState CurrentGameState = GameState.MainMenu;


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
			int modifier = instance.IsTopRowEven ? y % 2 : (y + 1) % 2;
			int bubbleCount = GAMEWIDTH - modifier;

			for (int x = 0; x < bubbleCount; x++)
			{
				var cur = instance.GameBoard[x, y];
				if (cur != null)
				{
					float offsetX = (modifier == 0) ? 0 : TILESIZE / 2;
					float offsetY = y * TILESIZE * 0.866f; // Reduce vertical gap for hexagonal layout (0.866f = sqrt(3)/2)
					cur.Position = new Vector2(x * TILESIZE + offsetX, offsetY);
				}
			}
		}
	}

	public static void printgameboard()
	{
		// Print GameBoard structure to console
		Console.WriteLine("GameBoard Visualization:");
		for (int y = 0; y < GAMEHEIGHT; y++)
		{
			string row = "";
			string type = IsRowEven(y) ? "even" : "odd ";
			for (int x = 0; x < GAMEWIDTH; x++)
			{
				row += Instance.GameBoard[x, y] != null ? "O " : ". ";
			}
			Console.WriteLine(type + ": " + row);
		}
	}

	public static void DropCeiling()
	{
		Console.WriteLine("Ceiling Dropped!");

		bool willLose = false;
		// Check lose state
		for (int x = 0; x < GAMEWIDTH; x++)
		{
			if (instance.GameBoard[x, GAMEHEIGHT - 1] != null)
			{
				willLose = true;
				break;
			}
		}

		instance.IsTopRowEven = !instance.IsTopRowEven;

		// Move all bubbles down by 1 row
		for (int y = GAMEHEIGHT - 1; y > 0; y--)
		{
			for (int x = 0; x < GAMEWIDTH; x++)
			{
				instance.GameBoard[x, y] = instance.GameBoard[x, y - 1];
			}
		}
		// clear old row
		for (int x = 0; x < GAMEWIDTH; x++)
		{
			instance.GameBoard[x, 0] = null;
		}

		// Generate the top row
		int bubbleCount = instance.IsTopRowEven ? GAMEWIDTH : GAMEWIDTH - 1;
		for (int x = 0; x < bubbleCount; x++)
		{
			instance.GameBoard[x, 0] = new Bubble(new Vector2(0f, 0f));
		}

		// Reset the shot counter
		instance.ShotCounter = 0;

		// Re-render the game board after shifting
		rendergameboard();

		if (willLose)
		{
			instance.CurrentGameState = GameState.GameLose;
		}
	}

	public static bool IsRowEven(int rowIndex)
	{
		return instance.IsTopRowEven ? (rowIndex % 2 == 0) : (rowIndex % 2 != 0);
	}

	public static bool IsGameBoardEmpty()
	{
		for (int y = 0; y < GAMEHEIGHT; y++)
		{
			for (int x = 0; x < GAMEWIDTH; x++)
			{
				if (Instance.GameBoard[x, y] != null)
				{
					return false;
				}
			}
		}

		return true;
	}

	public static void TriggerBubbleDestroyed(Vector2 position)
	{
		OnBubbleDestroyed?.Invoke(position);
	}

	public static bool RandomByPercent(int percent)
	{
		return Instance.Random.Next(100) < percent;
	}

	public void UpdateCombo(int combo, Vector2 position)
	{
		if (combo > 1) // Only show combos of 2 or more
		{
			LastComboCount = combo;
			LastComboPosition = position;
			ComboDisplayTimer = ComboDisplayDuration;
		}
	}

	public void ReduceComboTimer(GameTime gameTime)
	{
		if (ComboDisplayTimer > 0)
		{
			ComboDisplayTimer -= gameTime.ElapsedGameTime.TotalSeconds;
		}
	}


}