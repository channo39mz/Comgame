﻿using Comgame.GameObject;
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

    private Rectangle playButtonRect;
    private Rectangle exitButtonRect;
    private Rectangle playAgainButtonRect;
    private Rectangle restartButtonRect;
    private Rectangle menuButtonRect;



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

        _bubbleTextures = new Dictionary<Bubble.BubbleColor, Texture2D>
        {
            { Bubble.BubbleColor.RED, Content.Load<Texture2D>("bubble_red") },
            { Bubble.BubbleColor.BLUE, Content.Load<Texture2D>("bubble_blue") },
            { Bubble.BubbleColor.GREEN, Content.Load<Texture2D>("bubble_green") },
            { Bubble.BubbleColor.YELLOW, Content.Load<Texture2D>("bubble_yellow") }
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

        _rectTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _rectTexture.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("GameFont");

        Reset();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        Singleton.Instance.CurrentKey = keyboardState;

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
        else if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameLose)
        {
            DrawGameWonScreen();
        }


        _spriteBatch.End();
        base.Draw(gameTime);
    }

    protected void Reset(GameTime gameTime = null)
    {
        if (Singleton.Instance.Score > readHighScore()) saveHighScore();
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
                Singleton.Instance.HighScore = int.Parse(lines[0]);
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

        // Draw launcher threshold
        _spriteBatch.Draw(_rectTexture, new Vector2(0f, Singleton.GAMEHEIGHT * Singleton.TILESIZE), null, Color.White, 0f, Vector2.Zero,
            new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE, 1f), SpriteEffects.None, 0f);

        // Draw launcher
        _launcher.Draw(_spriteBatch);

        // Draw text
        _spriteBatch.DrawString(_font, "Next: ", new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE), Color.White);

        //draw score text
        _spriteBatch.DrawString(_font, "Score: " + Singleton.Instance.Score, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 100), Color.White);

        //draw high score text
        _spriteBatch.DrawString(_font, "Highscore: " + Singleton.Instance.HighScore, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 200), Color.White);

        //draw time elapsed
        _spriteBatch.DrawString(_font, "Time: " + Math.Round(gameTime.TotalGameTime.TotalSeconds - Singleton.Instance.GameStartTime), new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 300), Color.White);

        //draw best time
        _spriteBatch.DrawString(_font, "Best Time: " + Singleton.Instance.BestTime, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE + 400), Color.White);
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
        _spriteBatch.DrawString(_font, gameWonText, new Vector2((_graphics.PreferredBackBufferWidth - _font.MeasureString(gameWonText).X) / 2, 150), Color.Red);

        DrawButton(playAgainButtonRect, "Play Again");
        DrawButton(menuButtonRect, "Main Menu");
    }
}
