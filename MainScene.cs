using Comgame.GameObject;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
    private Song _song;
    private SoundEffect _winSound;
    private SoundEffect _loseSound;
    private float _volume = 0.25f;
    private bool _effPlayTime = false;
    private bool _isBGMStop = false;
    private Random _random = new Random();
    private int _randomNum;

    private Rectangle _playButtonRect;
    private Rectangle _exitButtonRect;
    private Rectangle _playAgainButtonRect;
    private Rectangle _restartButtonRect;
    private Rectangle _menuButtonRect;
    private Texture2D _explosionTexture;
    private List<Explosion> _activeExplosions = new List<Explosion>();

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

        int buttonWidth = 200;
        int buttonHeight = 60;
        int centerX = (_graphics.PreferredBackBufferWidth - buttonWidth) / 2;
        int centerY = _graphics.PreferredBackBufferHeight / 2;

        _playButtonRect = new Rectangle(centerX, centerY - 50, buttonWidth, buttonHeight);
        _exitButtonRect = new Rectangle(centerX, centerY + 50, buttonWidth, buttonHeight);

        _playAgainButtonRect = new Rectangle(centerX, _graphics.PreferredBackBufferHeight / 2, buttonWidth, buttonHeight);
        _restartButtonRect = new Rectangle(centerX, _graphics.PreferredBackBufferHeight / 2, buttonWidth, buttonHeight);
        _menuButtonRect = new Rectangle(centerX, _graphics.PreferredBackBufferHeight / 2 + 100, buttonWidth, buttonHeight);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _randomNum = _random.Next(0, 10);
        _song = _randomNum % 2 == 0 ? Content.Load<Song>("Audio/bgm_main") : Content.Load<Song>("Audio/bgm_main2");
        Console.WriteLine(_randomNum % 2);
        Singleton.Instance.ExplodedSound = Content.Load<SoundEffect>("Audio/exploded");
        Singleton.Instance.CeilingDropSound = Content.Load<SoundEffect>("Audio/newroll");
        Singleton.Instance.BlackholeSound = Content.Load<SoundEffect>("Audio/blackhole");
        _winSound = Content.Load<SoundEffect>("Audio/win");
        _loseSound = Content.Load<SoundEffect>("Audio/fail");
        MediaPlayer.Play(_song); // Background Music play
        MediaPlayer.Volume = _volume; // Background Music Volume
        _bubbleTextures = new Dictionary<Bubble.BubbleColor, Texture2D>
        {
            { Bubble.BubbleColor.RED, Content.Load<Texture2D>("bubble_red") },
            { Bubble.BubbleColor.BLUE, Content.Load<Texture2D>("bubble_blue") },
            { Bubble.BubbleColor.GREEN, Content.Load<Texture2D>("bubble_green") },
            { Bubble.BubbleColor.YELLOW, Content.Load<Texture2D>("bubble_yellow") },
            { Bubble.BubbleColor.BLACKHOLE, Content.Load<Texture2D>("blackhole") }
        };
        Bubble.LoadTextures(_bubbleTextures);

        _backgroundTextures = new Texture2D[]
        {
            Content.Load<Texture2D>("bg_0"),
            Content.Load<Texture2D>("bg_1"),
            Content.Load<Texture2D>("bg_2"),
            Content.Load<Texture2D>("bg_3"),
            Content.Load<Texture2D>("bg_4"),
            Content.Load<Texture2D>("bg_5"),
        };

        _launcherTexture = Content.Load<Texture2D>("launcher");
        _explosionTexture = Content.Load<Texture2D>("bk_explo_one");

        _rectTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _rectTexture.SetData([Color.White]);

        _font = Content.Load<SpriteFont>("GameFont");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        Singleton.Instance.CurrentKey = keyboardState;

        if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon && !_effPlayTime)
        {
            _winSound.Play(0.6f, 0.0f, 0.0f);
            MediaPlayer.Stop();
            Console.WriteLine(_effPlayTime);
            MediaPlayer.IsRepeating = true;
            _isBGMStop = true;
            _effPlayTime = true;
        }
        if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose && !_effPlayTime)
        {
            _loseSound.Play(0.7f, 0.0f, 0.0f);
            MediaPlayer.Stop();
            Console.WriteLine(_effPlayTime);
            MediaPlayer.IsRepeating = true;
            _isBGMStop = true;
            _effPlayTime = true;
        }
        if (_isBGMStop && !(Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose || Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon))
        {
            _isBGMStop = false;
            MediaPlayer.Play(_song);
            MediaPlayer.Volume = _volume;
        }

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (Singleton.Instance.CurrentGameState == Singleton.GameState.MainMenu)
        {
            HandleMenuInput(gameTime);
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.InGame)
        {
            if (Singleton.Instance.PreviousKey.IsKeyDown(Keys.R) && Singleton.Instance.CurrentKey.IsKeyUp(Keys.R))
            {
                Reset(gameTime);
            }

            _launcher.Update(gameTime);
            _effPlayTime = false;

            if (Singleton.Instance.IsGameBoardEmpty())
            {
                Singleton.Instance.CurrentGameState = Singleton.GameState.GameWon;
            }

            // Update background animation timer
            _bgTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_bgTimer >= _bgInterval)
            {
                _currentBgIndex = (_currentBgIndex + 1) % _backgroundTextures.Length;
                _bgTimer = 0;
            }
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose)
        {
            HandleGameOverInput(gameTime);
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon)
        {
            HandleGameWonInput(gameTime);
        }

        Singleton.Instance.PreviousKey = Singleton.Instance.CurrentKey;

        // Update active explosions
        for (int i = _activeExplosions.Count - 1; i >= 0; i--)
        {
            _activeExplosions[i].Update(gameTime);
            if (_activeExplosions[i].IsFinished)
            {
                _activeExplosions.RemoveAt(i);
            }
        }

        // Register event for bubble destruction
        Singleton.OnBubbleDestroyed += pos =>
        {
            _activeExplosions.Add(new Explosion(
                _explosionTexture,
                pos,
                64,    // Frame width
                64,    // Frame height
                36,    // Columns in sprite sheet
                1,     // Rows in sprite sheet
                -0.01f // Frame time
            ));
        };

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        if (Singleton.Instance.CurrentGameState == Singleton.GameState.MainMenu)
        {
            DrawMainMenu();
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.InGame)
        {
            DrawGame(gameTime);
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose)
        {
            DrawGameOverScreen();
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon)
        {
            DrawGameWonScreen();
        }

        // Draw active explosions
        foreach (var explosion in _activeExplosions)
        {
            explosion.Draw(_spriteBatch);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    protected void Reset(GameTime gameTime = null)
    {
        LoadBestTime();
        LoadHighScore();
        _launcher = new Launcher(_launcherTexture, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE / 2, (Singleton.GAMEHEIGHT + 1) * Singleton.TILESIZE));
        Singleton.Instance.GameBoard = new Bubble[Singleton.GAMEWIDTH, Singleton.GAMEHEIGHT];

        if (gameTime != null)
        {
            Singleton.Instance.GameStartTime = gameTime.TotalGameTime.TotalSeconds;
            Singleton.Instance.ShotCounter = 0;
            Singleton.Instance.IsTopRowEven = true;
            Singleton.Instance.Score = 0;
        }

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

    protected void LoadHighScore()
    {
        // Load score from file
        if (File.Exists(Singleton.SCOREFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.SCOREFILE);
            if (lines.Length > 0)
            {
                Singleton.Instance.HighScore = int.Parse(lines[0]);
            }
        }
    }

    protected void LoadBestTime()
    {
        // Load score from file
        if (File.Exists(Singleton.BESTTIMEFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.BESTTIMEFILE);
            if (lines.Length > 0)
            {
                string bestTimeStr = lines[0];
                if (bestTimeStr == "Inf")
                {
                    Singleton.Instance.BestTime = double.PositiveInfinity;
                }
                else
                {
                    Singleton.Instance.BestTime = double.Parse(bestTimeStr);
                }
            }
        }
    }

    protected void SaveHighScore()
    {
        // Save score to file
        File.WriteAllText(Singleton.SCOREFILE, Singleton.Instance.Score.ToString());
    }

    protected void SaveBestTime(GameTime gameTime)
    {
        // Save score to file
        File.WriteAllText(Singleton.BESTTIMEFILE, (gameTime.TotalGameTime.TotalSeconds - Singleton.Instance.GameStartTime).ToString());
    }

    public void IncreaseScore(int score)
    {
        Singleton.Instance.Score += score;
        Console.WriteLine(Singleton.Instance.Score);
    }

    private void HandleMenuInput(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            Point mousePoint = new Point(mouseState.X, mouseState.Y);
            if (_playButtonRect.Contains(mousePoint))
            {
                Singleton.Instance.GameStartTime = gameTime.TotalGameTime.TotalSeconds;
                Reset(gameTime);
                Singleton.Instance.CurrentGameState = Singleton.GameState.InGame;

            }
            else if (_exitButtonRect.Contains(mousePoint))
            {
                Exit();
            }
        }
    }

    private void HandleGameOverInput(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            Point mousePoint = new Point(mouseState.X, mouseState.Y);
            if (_restartButtonRect.Contains(mousePoint))
            {
                Reset(gameTime);
                Singleton.Instance.CurrentGameState = Singleton.GameState.InGame;
            }
            else if (_menuButtonRect.Contains(mousePoint))
            {
                Singleton.Instance.CurrentGameState = Singleton.GameState.MainMenu;
            }
        }
    }

    private void HandleGameWonInput(GameTime gameTime)
    {
        if (Singleton.Instance.Score > Singleton.Instance.HighScore)
            SaveHighScore();
        if ((gameTime.TotalGameTime.TotalSeconds - Singleton.Instance.GameStartTime) < Singleton.Instance.BestTime)
            SaveBestTime(gameTime);

        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            Point mousePoint = new Point(mouseState.X, mouseState.Y);
            if (_playAgainButtonRect.Contains(mousePoint))
            {
                Reset(gameTime);
                Singleton.Instance.CurrentGameState = Singleton.GameState.InGame;
            }
            else if (_menuButtonRect.Contains(mousePoint))
            {
                Singleton.Instance.CurrentGameState = Singleton.GameState.MainMenu;
            }
        }
    }

    private void DrawMainMenu()
    {
        string titleText = "Shoot that asteroid like those balls!";
        _spriteBatch.DrawString(_font, titleText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(titleText).X) / 2, 120), Color.White);

        DrawButton(_playButtonRect, "Play");
        DrawButton(_exitButtonRect, "Exit");
    }

    private void DrawButton(Rectangle rect, string text)
    {
        _spriteBatch.Draw(_rectTexture, rect, Color.Gray);
        Vector2 textSize = _font.MeasureString(text);
        Vector2 textPosition = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2,
            rect.Y + (rect.Height - textSize.Y) / 2
        );
        _spriteBatch.DrawString(_font, text, textPosition, Color.White);
    }

    private void DrawGame(GameTime gameTime)
    {

        // Calculate the scale factors to fit the background to the game area
        float scaleX = (float)(Singleton.GAMEWIDTH * Singleton.TILESIZE) / _backgroundTextures[_currentBgIndex].Width;
        float scaleY = (float)(Singleton.SCREENHEIGHT * Singleton.TILESIZE) / _backgroundTextures[_currentBgIndex].Height;

        // Draw game board background
        _spriteBatch.Draw(_backgroundTextures[_currentBgIndex], Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

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

        // Reduce combo display time
        Singleton.Instance.ReduceComboTimer(gameTime);

        // Draw combo text if active
        if (Singleton.Instance.ComboDisplayTimer > 0)
        {
            string comboText = $"x{Singleton.Instance.LastComboCount}!";
            Vector2 textSize = _font.MeasureString(comboText);
            // Ensure the text doesn't go off-screen
            Vector2 drawPosition = Singleton.Instance.LastComboPosition - new Vector2(textSize.X / 2, textSize.Y);
            drawPosition.X = MathHelper.Clamp(drawPosition.X, 0, Singleton.GAMEWIDTH * Singleton.TILESIZE - textSize.X);
            drawPosition.Y = MathHelper.Clamp(drawPosition.Y, 0, Singleton.GAMEHEIGHT * Singleton.TILESIZE - textSize.Y);
            _spriteBatch.DrawString(_font, comboText, drawPosition, Color.White);
        }

        // Draw launcher threshold
        _spriteBatch.Draw(_rectTexture, new Vector2(0f, Singleton.GAMEHEIGHT * Singleton.TILESIZE), null, Color.White, 0f, Vector2.Zero,
            new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE, 1f), SpriteEffects.None, 0f);

        // Draw launcher
        _launcher.Draw(_spriteBatch);

        // Draw text
        _spriteBatch.DrawString(_font, "Next: ", new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 8, Singleton.TILESIZE), Color.White);

        //draw score text
        _spriteBatch.DrawString(_font, "Score: " + Singleton.Instance.Score, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 8, Singleton.TILESIZE + 100), Color.White);

        //draw high score text
        _spriteBatch.DrawString(_font, "Highscore: " + Singleton.Instance.HighScore, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 8, Singleton.TILESIZE + 150), Color.White);

        //draw time elapsed
        _spriteBatch.DrawString(_font, "Time: " + Math.Round(gameTime.TotalGameTime.TotalSeconds - Singleton.Instance.GameStartTime), new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 8, Singleton.TILESIZE + 250), Color.White);

        //draw best time
        string bestTimeText = Singleton.Instance.BestTime == double.PositiveInfinity ? "Best Time: N/A" : "Best Time: " + Math.Round(Singleton.Instance.BestTime);
        _spriteBatch.DrawString(_font, bestTimeText, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 8, Singleton.TILESIZE + 300), Color.White);
    }

    private void DrawGameOverScreen()
    {
        string gameOverText = "Game Over!";
        _spriteBatch.DrawString(_font, gameOverText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(gameOverText).X) / 2, 150), Color.Red);

        DrawButton(_restartButtonRect, "Restart");
        DrawButton(_menuButtonRect, "Main Menu");
    }

    private void DrawGameWonScreen()
    {
        string gameWonText = "You Won!";
        _spriteBatch.DrawString(_font, gameWonText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(gameWonText).X) / 2, 150), Color.Green);

        DrawButton(_playAgainButtonRect, "Play Again");
        DrawButton(_menuButtonRect, "Main Menu");
    }
}
