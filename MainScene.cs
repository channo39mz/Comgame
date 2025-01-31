using Comgame.GameObject;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Comgame;

public class MainScene : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _font;
    private Texture2D _rectTexture;
    private Texture2D _launcherTexture;
    private Launcher _launcher;
    private Dictionary<Bubble.BubbleColor, Texture2D> _bubbleTextures;
    private Texture2D[] _backgroundTextures;
    private int _currentBgIndex = 0;
    private double _bgTimer = 0;
    private double _bgInterval = 0.5; // Time interval in seconds (adjust as needed)

    // private int Singleton.Instance._score= 0;


    public MainScene()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = Singleton.SCREENWIDTH * Singleton.TILESIZE;
        _graphics.PreferredBackBufferHeight = Singleton.SCREENHEIGHT * Singleton.TILESIZE;
        _graphics.ApplyChanges();

        Window.Title = Singleton.WINDOWTITLE;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _bubbleTextures = new Dictionary<Bubble.BubbleColor, Texture2D>
        {
            { Bubble.BubbleColor.RED, Content.Load<Texture2D>("bubble_red") },
            { Bubble.BubbleColor.BLUE, Content.Load<Texture2D>("bubble_blue") },
            { Bubble.BubbleColor.GREEN, Content.Load<Texture2D>("bubble_green") },
            { Bubble.BubbleColor.YELLOW, Content.Load<Texture2D>("bubble_yellow") }
        };
        Bubble.LoadTextures(_bubbleTextures);

        _backgroundTextures = new Texture2D[6]
        {
            Content.Load<Texture2D>("bg_0"),
            Content.Load<Texture2D>("bg_1"),
            Content.Load<Texture2D>("bg_2"),
            Content.Load<Texture2D>("bg_3"),
            Content.Load<Texture2D>("bg_4"),
            Content.Load<Texture2D>("bg_5"),
        };

        _launcherTexture = Content.Load<Texture2D>("launcher");

        _rectTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _rectTexture.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("GameFont");

        Reset();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        Singleton.Instance.CurrentKey = keyboardState;

        if (keyboardState.IsKeyDown(Keys.Escape)){
            if (Singleton.Instance._score> readHighScore()) saveHighScore();
            Exit();
        }



        _launcher.Update(gameTime);

        // Update background animation timer
        _bgTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_bgTimer >= _bgInterval)
        {
            _currentBgIndex = (_currentBgIndex + 1) % _backgroundTextures.Length;
            _bgTimer = 0;
        }

        Singleton.Instance.PreviousKey = Singleton.Instance.CurrentKey;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        // Calculate the scale factors to fit the background to the game area
        float scaleX = (float)(Singleton.GAMEWIDTH * Singleton.TILESIZE) / _backgroundTextures[_currentBgIndex].Width;
        float scaleY = (float)(Singleton.SCREENHEIGHT * Singleton.TILESIZE) / _backgroundTextures[_currentBgIndex].Height;

        // Draw game board background
        _spriteBatch.Draw(_backgroundTextures[_currentBgIndex], Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
            new Vector2(scaleX,scaleY), SpriteEffects.None, 0f);

        // Draw score area background
        _spriteBatch.Draw(_rectTexture, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE, 0f), null, Color.DimGray, 0f, Vector2.Zero,
            new Vector2(Singleton.SCOREWIDTH * Singleton.TILESIZE, Singleton.SCREENHEIGHT * Singleton.TILESIZE), SpriteEffects.None, 0f);

        // Draw bubbles
        for (int y = 0; y < Singleton.GAMEHEIGHT; y++)
        {
            for (int x = 0; x < Singleton.GAMEWIDTH; x++)
            {
                if (Singleton.Instance.GameBoard[x, y] != null)
                {
                    Singleton.Instance.GameBoard[x, y].Draw(_spriteBatch);
                }
            }
        }

        // Draw launcher
        _launcher.Draw(_spriteBatch);

        // Draw text
        _spriteBatch.DrawString(_font, "Next: ", new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE), Color.White);

        //draw score text
        _spriteBatch.DrawString(_font, "Score: " + Singleton.Instance._score, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 100), Color.White);

        //draw high score text
        _spriteBatch.DrawString(_font, "Highscore: " + Singleton.Instance._highScore, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 200), Color.White);


        //draw time elapsed
        _spriteBatch.DrawString(_font, "Time: " + gameTime.TotalGameTime.TotalSeconds!, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 300), Color.White);

        //draw best time
        _spriteBatch.DrawString(_font, "Best Time: " + Singleton.Instance._bestTime, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 400), Color.White);

        _spriteBatch.End();
        base.Draw(gameTime);


    }

    protected void Reset()
    {
        if (Singleton.Instance._score> readHighScore()) saveHighScore();
        loadHighScore();
        _launcher = new Launcher(_launcherTexture, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE / 2, (Singleton.GAMEHEIGHT + 1) * Singleton.TILESIZE), this);
        Singleton.Instance.GameBoard = new Bubble[Singleton.GAMEWIDTH, Singleton.GAMEHEIGHT];

        // Initialize bubbles in a zigzag pattern with decreasing count per row
        for (int y = 0; y < Singleton.INITIALROWS; y++)
        {
            int bubbleCount = Singleton.GAMEWIDTH - (y % 2);
            for (int x = 0; x < bubbleCount; x++)
            {
                float offsetX = (y % 2 == 0) ? 0 : Singleton.TILESIZE / 2;
                float offsetY = y * Singleton.TILESIZE * 0.866f; // Reduce vertical gap for hexagonal layout (0.866f = sqrt(3)/2)
                var bubble = new Bubble(new Vector2(x * Singleton.TILESIZE + offsetX, offsetY));

                Singleton.Instance.GameBoard[x, y] = bubble;
            }
        }

        // Print GameBoard structure to console
        Console.WriteLine("GameBoard Initialization:");
        for (int y = 0; y < Singleton.INITIALROWS; y++)
        {
            string row = "";
            for (int x = 0; x < Singleton.GAMEWIDTH; x++)
            {
                row += Singleton.Instance.GameBoard[x, y] != null ? "O " : ". ";
            }
            Console.WriteLine(row);
        }
    }

    protected void loadHighScore()
    {
        // Load score from file
        if (File.Exists(Singleton.SCOREFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.SCOREFILE);
            if (lines.Length > 0)
            {
                Singleton.Instance._highScore= int.Parse(lines[0]);
            }
        }
    }

    protected void loadBestTime()
    {
        // Load score from file
        if (File.Exists(Singleton.BESTTIMEFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.BESTTIMEFILE);
            if (lines.Length > 0)
            {
                Singleton.Instance._highScore= int.Parse(lines[0]);
            }
        }
    }

    protected void saveHighScore()
    {
        // Save score to file
        File.WriteAllText(Singleton.SCOREFILE, Singleton.Instance._score.ToString());
    }

    protected int readHighScore()
    {
        // Read score from file
        if (File.Exists(Singleton.SCOREFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.SCOREFILE);
            if (lines.Length > 0)
            {
                return int.Parse(lines[0]);
            }
        }
        return 0;
    }

    public void IncreaseScore(int score)
    {
        Singleton.Instance._score+= score;
        Console.WriteLine(Singleton.Instance._score);
    }
}
