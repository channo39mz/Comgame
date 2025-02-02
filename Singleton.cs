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

	// Constants
	// General Constants
	public const string WINDOWTITLE = "Shoot that asteroid like those balls!";
	public const string SCOREFILE = "./Content/highscore.txt";
	public const string BESTTIMEFILE = "./Content/besttime.txt";
	public const int TILESIZE = 48; // 48x48 pixels

	// Bubble Dimensions
	public const int GAMEWIDTH = 8;
	public const int GAMEHEIGHT = 10;

	// Screen Dimensions
	public const int SCOREWIDTH = 5;
	public const int LAUNCHERHEIGHT = 2;
	public const int SCREENWIDTH = GAMEWIDTH + SCOREWIDTH;
	public const int SCREENHEIGHT = GAMEHEIGHT + LAUNCHERHEIGHT;

	// Game Constants
	public const int INITIALROWS = 4; // Initial rows of bubbles
	public const int SHOTS_BEFORE_DROP = 10; // Drop ceiling after n shots
	private const double COMBO_DISPLAY_DURATION = 1.0; // Combo display duration in seconds

	// Properties
	// Game Stats
	public int Score = 0;
	public int HighScore = 0;
	public double GameStartTime = 0;
	public double BestTime = 0;

	// Sound Effects
	public SoundEffect ExplodedSound;
	public SoundEffect CeilingDropSound;
	public SoundEffect BlackholeSound;

	// Game Objects
	public MovingBubble CurrentBubble; // The bubble that is currently being shot or loaded
	public MovingBubble NextBubble;
	public Bubble[,] GameBoard;

	// Input
	public KeyboardState PreviousKey;
	public KeyboardState CurrentKey;

	// Random
	public Random Random { get; } = new Random();

	// Combo tracking
	public int LastComboCount { get; private set; } = 0;
	public Vector2 LastComboPosition { get; private set; } = Vector2.Zero;
	public double ComboDisplayTimer { get; private set; } = 0;

	// Misc
	public int ShotCounter = 0; // Counter for number of shots fired
	public bool IsTopRowEven = true;
	public static event Action<Vector2> OnBubbleDestroyed;

	// Game states
	public enum GameState
	{
		MainMenu,
		InGame,
		GameWon,
		GameLose
	}

	public GameState CurrentGameState = GameState.MainMenu;

	private Singleton() { }

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

	// Methods
	public void RenderGameBoard()
	{
		for (int y = 0; y < GAMEHEIGHT; y++)
		{
			int modifier = IsTopRowEven ? y % 2 : (y + 1) % 2;
			int bubbleCount = GAMEWIDTH - modifier;

			for (int x = 0; x < bubbleCount; x++)
			{
				var cur = GameBoard[x, y];
				if (cur != null)
				{
					float offsetX = (modifier == 0) ? 0 : TILESIZE / 2;
					float offsetY = y * TILESIZE * 0.866f; // Reduce vertical gap for hexagonal layout (0.866f = sqrt(3)/2)
					cur.Position = new Vector2(x * TILESIZE + offsetX, offsetY);
				}
			}
		}
	}

	public void PrintGameBoard()
	{
		// Print GameBoard structure to console
		Console.WriteLine("GameBoard Visualization:");
		for (int y = 0; y < GAMEHEIGHT; y++)
		{
			string row = "";
			string type = IsRowEven(y) ? "even" : "odd ";
			for (int x = 0; x < GAMEWIDTH; x++)
			{
				row += GameBoard[x, y] != null ? "O " : ". ";
			}
			Console.WriteLine(type + ": " + row);
		}
	}

	public void DropCeiling()
	{
		Console.WriteLine("Ceiling Dropped!");
		CeilingDropSound.Play();

		bool willLose = false;
		// Check lose state
		for (int x = 0; x < GAMEWIDTH; x++)
		{
			if (GameBoard[x, GAMEHEIGHT - 1] != null)
			{
				willLose = true;
				break;
			}
		}

		IsTopRowEven = !IsTopRowEven;

		// Move all bubbles down by 1 row
		for (int y = GAMEHEIGHT - 1; y > 0; y--)
		{
			for (int x = 0; x < GAMEWIDTH; x++)
			{
				GameBoard[x, y] = GameBoard[x, y - 1];
			}
		}
		// clear old row
		for (int x = 0; x < GAMEWIDTH; x++)
		{
			GameBoard[x, 0] = null;
		}

		// Generate the top row
		int bubbleCount = IsTopRowEven ? GAMEWIDTH : GAMEWIDTH - 1;
		for (int x = 0; x < bubbleCount; x++)
		{
			GameBoard[x, 0] = new Bubble(new Vector2(0f, 0f));
		}

		// Reset the shot counter
		ShotCounter = 0;

		// Re-render the game board after shifting
		RenderGameBoard();

		if (willLose)
		{
			CurrentGameState = GameState.GameLose;
		}
	}

	public bool IsRowEven(int rowIndex)
	{
		return IsTopRowEven ? (rowIndex % 2 == 0) : (rowIndex % 2 != 0);
	}

	public bool IsGameBoardEmpty()
	{
		for (int y = 0; y < GAMEHEIGHT; y++)
		{
			for (int x = 0; x < GAMEWIDTH; x++)
			{
				if (GameBoard[x, y] != null)
				{
					return false;
				}
			}
		}

		return true;
	}

	public void TriggerBubbleDestroyed(Vector2 position)
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
			ComboDisplayTimer = COMBO_DISPLAY_DURATION;
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