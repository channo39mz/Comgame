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
    private Song song;
    private SoundEffect dropRow;
    private SoundEffect winSound;
    private SoundEffect loseSound;
    public SoundEffect exploded;
    private float volumn = 0.25f;
    private bool effPlayTime = false;
    private bool isBGMStop = false;
    private Random randoming = new Random();
    private int randomNum;

    private Rectangle playButtonRect;
    private Rectangle exitButtonRect;
    private Rectangle playAgainButtonRect;
    private Rectangle restartButtonRect;
    private Rectangle menuButtonRect;
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

        playButtonRect = new Rectangle(centerX, centerY - 50, buttonWidth, buttonHeight);
        exitButtonRect = new Rectangle(centerX, centerY + 50, buttonWidth, buttonHeight);

        playAgainButtonRect = new Rectangle(centerX, _graphics.PreferredBackBufferHeight / 2, buttonWidth, buttonHeight);
        restartButtonRect = new Rectangle(centerX, _graphics.PreferredBackBufferHeight / 2, buttonWidth, buttonHeight);
        menuButtonRect = new Rectangle(centerX, _graphics.PreferredBackBufferHeight / 2 + 100, buttonWidth, buttonHeight);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        randomNum = randoming.Next(0, 10);
        if (randomNum % 2 == 0)
        {
            song = Content.Load<Song>("Audio/bgm_main");
        }
        else
        {
            song = Content.Load<Song>("Audio/bgm_main2");
        }
        Console.WriteLine(randomNum % 2);
        Singleton.Instance.exploded = Content.Load<SoundEffect>("Audio/exploded");
        Singleton.Instance.dropRow = Content.Load<SoundEffect>("Audio/newroll");
        winSound = Content.Load<SoundEffect>("Audio/win");
        loseSound = Content.Load<SoundEffect>("Audio/fail");
        MediaPlayer.Play(song); //Backgound Music play
        MediaPlayer.Volume = volumn; //Background Music Volumn
        _bubbleTextures = new Dictionary<Bubble.BubbleColor, Texture2D>
        {
            { Bubble.BubbleColor.RED, Content.Load<Texture2D>("bubble_red") },
            { Bubble.BubbleColor.BLUE, Content.Load<Texture2D>("bubble_blue") },
            { Bubble.BubbleColor.GREEN, Content.Load<Texture2D>("bubble_green") },
            { Bubble.BubbleColor.YELLOW, Content.Load<Texture2D>("bubble_yellow") },
            { Bubble.BubbleColor.BLACKHOLE, Content.Load<Texture2D>("blackhole") }
        };
        Bubble.LoadTextures(_bubbleTextures);

        _backgroundTextures =
        [
            Content.Load<Texture2D>("bg_0"),
            Content.Load<Texture2D>("bg_1"),
            Content.Load<Texture2D>("bg_2"),
            Content.Load<Texture2D>("bg_3"),
            Content.Load<Texture2D>("bg_4"),
            Content.Load<Texture2D>("bg_5"),
        ];

        _launcherTexture = Content.Load<Texture2D>("launcher");

        _explosionTexture = Content.Load<Texture2D>("bk_explo_one");

        _rectTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _rectTexture.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("GameFont");

        Reset();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        Singleton.Instance.CurrentKey = keyboardState;

        if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon && effPlayTime == false)
        {
            winSound.Play(0.25f, 0.0f, 0.0f);
            MediaPlayer.Stop();
            Console.WriteLine(effPlayTime);
            MediaPlayer.IsRepeating = true;
            isBGMStop = true;
            effPlayTime = true;
        }
        if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose && effPlayTime == false)
        {
            loseSound.Play(0.25f, 0.0f, 0.0f);
            MediaPlayer.Stop();
            Console.WriteLine(effPlayTime);
            MediaPlayer.IsRepeating = true;
            isBGMStop = true;
            effPlayTime = true;
        }
        if (isBGMStop == true && !(Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose || Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon))
        {
            isBGMStop = false;
            MediaPlayer.Play(song);
            MediaPlayer.Volume = volumn;
        }


        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            if (Singleton.Instance.Score > readHighScore()) saveHighScore();
            Exit();
        }

        if (Singleton.Instance.CurrentGameState == Singleton.GameState.MainMenu)
        {
            HandleMenuInput(gameTime);
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.InGame)
        {
            _launcher.Update(gameTime);
            effPlayTime = false;

            // Bypass
            // if (Singleton.Instance.ShotCounter == 10)
            // {
            //     Singleton.Instance.CurrentGameState = Singleton.GameState.GameWon;
            // }

            if (Singleton.IsGameBoardEmpty())
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

            // Singleton.Instance.CeilingDropTimer += gameTime.ElapsedGameTime.TotalSeconds;

            // if (Singleton.Instance.CeilingDropTimer >= Singleton.CEILING_DROP_INTERVAL)
            // {
            //     Singleton.DropCeiling();
            //     Singleton.Instance.CeilingDropTimer = 0.0; // Reset timer after dropping ceiling
            // }
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose)
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePoint = new Point(mouseState.X, mouseState.Y);
                if (restartButtonRect.Contains(mousePoint))
                {
                    Reset(gameTime);
                    Singleton.Instance.CurrentGameState = Singleton.GameState.InGame;
                }
                else if (menuButtonRect.Contains(mousePoint))
                {
                    Singleton.Instance.CurrentGameState = Singleton.GameState.MainMenu;
                }
            }
        }
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWon)
        {
            if (Singleton.Instance.Score > readHighScore())
                saveHighScore();
            if ((gameTime.TotalGameTime.TotalSeconds - Singleton.Instance.GameStartTime) < readBestTime())
                saveBestTime(gameTime);

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePoint = new Point(mouseState.X, mouseState.Y);
                if (playAgainButtonRect.Contains(mousePoint))
                {
                    Reset(gameTime);
                    Singleton.Instance.CurrentGameState = Singleton.GameState.InGame;
                }
                else if (menuButtonRect.Contains(mousePoint))
                {
                    Singleton.Instance.CurrentGameState = Singleton.GameState.MainMenu;
                }
            }
        }

        Singleton.Instance.PreviousKey = Singleton.Instance.CurrentKey;

        // เพิ่มในส่วน Update
        for (int i = _activeExplosions.Count - 1; i >= 0; i--)
        {
            _activeExplosions[i].Update(gameTime);
            if (_activeExplosions[i].IsFinished)
            {
                _activeExplosions.RemoveAt(i);
            }
        }

        // ลงทะเบียน Event ใน Initialize หรือ LoadContent
        Singleton.OnBubbleDestroyed += pos =>
        {
            _activeExplosions.Add(new Explosion(
                _explosionTexture,
                pos,
                64,    // ความกว้างของแต่ละเฟรม
                64,    // ความสูงของแต่ละเฟรม
                36,     // จำนวนคอลัมน์ในสไปรท์ชีต
                1,     // จำนวนแถวในสไปรท์ชีต
                -0.01f  // เวลาแสดงแต่ละเฟรม (วินาที)
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

        // เพิ่มในส่วน Draw หลังจากวาด Bubble
        foreach (var explosion in _activeExplosions)
        {
            explosion.Draw(_spriteBatch);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    protected void Reset(GameTime gameTime = null)
    {
        loadBestTime();
        loadHighScore();
        _launcher = new Launcher(_launcherTexture, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE / 2, (Singleton.GAMEHEIGHT + 1) * Singleton.TILESIZE));
        Singleton.Instance.GameBoard = new Bubble[Singleton.GAMEWIDTH, Singleton.GAMEHEIGHT];

        if (gameTime != null)
        {
            Singleton.Instance.GameStartTime = gameTime.TotalGameTime.TotalSeconds;
            Singleton.Instance.ShotCounter = 0;
            Singleton.Instance.IsTopRowEven = true;
            Singleton.Instance.CeilingDropTimer = 0.0;
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

    protected void loadHighScore()
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

    protected void loadBestTime()
    {
        // Load score from file
        if (File.Exists(Singleton.BESTTIMEFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.BESTTIMEFILE);
            if (lines.Length > 0)
            {
                Singleton.Instance.BestTime = double.Parse(lines[0]);
            }
        }
    }

    protected void saveHighScore()
    {
        // Save score to file
        File.WriteAllText(Singleton.SCOREFILE, Singleton.Instance.Score.ToString());
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

    protected void saveBestTime(GameTime gameTime)
    {
        // Save score to file
        File.WriteAllText(Singleton.BESTTIMEFILE, (gameTime.TotalGameTime.TotalSeconds - Singleton.Instance.GameStartTime).ToString());
    }

    protected double readBestTime()
    {
        // Read score from file
        if (File.Exists(Singleton.BESTTIMEFILE))
        {
            string[] lines = File.ReadAllLines(Singleton.BESTTIMEFILE);
            if (lines.Length > 0)
            {
                return double.Parse(lines[0]);
            }
        }
        return 0;
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
            if (playButtonRect.Contains(mousePoint))
            {
                Singleton.Instance.GameStartTime = gameTime.TotalGameTime.TotalSeconds;
                Reset(gameTime);
                Singleton.Instance.CurrentGameState = Singleton.GameState.InGame;

            }
            else if (exitButtonRect.Contains(mousePoint))
            {
                Exit();
            }
        }
    }

    private void DrawMainMenu()
    {
        string titleText = "Puzzle Bobble";
        _spriteBatch.DrawString(_font, titleText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(titleText).X) / 2, 120), Color.White);

        DrawButton(playButtonRect, "Play");
        DrawButton(exitButtonRect, "Exit");
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
        _spriteBatch.DrawString(_font, "Best Time: " + Math.Round(Singleton.Instance.BestTime), new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 8, Singleton.TILESIZE + 300), Color.White);
    }

    private void DrawGameOverScreen()
    {
        string gameOverText = "Game Over!";
        _spriteBatch.DrawString(_font, gameOverText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(gameOverText).X) / 2, 150), Color.Red);

        DrawButton(restartButtonRect, "Restart");
        DrawButton(menuButtonRect, "Main Menu");
    }

    private void DrawGameWonScreen()
    {
        string gameWonText = "You Won!";
        _spriteBatch.DrawString(_font, gameWonText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(gameWonText).X) / 2, 150), Color.Green);

        DrawButton(playAgainButtonRect, "Play Again");
        DrawButton(menuButtonRect, "Main Menu");
    }
}
