using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Comgame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _bubbleTexture;
    private Texture2D _launcherTexture;
    private Vector2 _launcherPosition;
    private Vector2 _bubblePosition;
    private Vector2 _bubbleVelocity;
    private Random _random = new Random();

    private const int BubbleSize = 32;
    private int Rows = 10;
    private int Columns = 10;
    private Tile[,] _grid;

    private double _dropTimer;
    private double _dropInterval = 5.0;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Initialize grid and launcher
        _grid = new Tile[Rows, Columns];
        _launcherPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - BubbleSize);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load textures
        _bubbleTexture = Content.Load<Texture2D>("bubble2");
        _launcherTexture = Content.Load<Texture2D>("luncher");

        // Create starting pattern of bubbles
        for (int row = 0; row < Rows / 2; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (row % 2 == 0 || col < Columns - 1)
                {
                    //_grid[row, col] = new Tile(_bubbleTexture, new Vector2(col * BubbleSize, row * BubbleSize), _random.Next(3));
                    _grid[row, col] = new Tile(_bubbleTexture, new Vector2(col * BubbleSize, row * BubbleSize + 50), _random.Next(3));
                }
            }
        }
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (_grid[row, col] != null)
                {
                    Console.WriteLine($"Bubble at [{row}, {col}] - Position: {_grid[row, col].Position}, Color: {_grid[row, col].Color}");
                }
            }
        }

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();

        // Launcher movement
        if (keyboardState.IsKeyDown(Keys.Left))
            _launcherPosition.X -= 5;
        if (keyboardState.IsKeyDown(Keys.Right))
            _launcherPosition.X += 5;

        // Clamp launcher position
        _launcherPosition.X = Math.Clamp(_launcherPosition.X, 0, GraphicsDevice.Viewport.Width - BubbleSize);

        // Shooting bubble
        if (keyboardState.IsKeyDown(Keys.Space) && _bubblePosition == Vector2.Zero)
        {
            _bubblePosition = _launcherPosition;
            _bubbleVelocity = new Vector2((float)Math.Cos(0), (float)Math.Sin(0)) * 5f;
        }

        // Update bubble movement
        if (_bubblePosition != Vector2.Zero)
        {
            _bubblePosition += _bubbleVelocity;

            // Detect collision with grid
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (_grid[row, col] != null && Vector2.Distance(_bubblePosition, _grid[row, col].Position) < BubbleSize)
                    {
                        PlaceBubble(row, col);
                        CheckMatches(row, col);
                        _bubblePosition = Vector2.Zero;
                        return;
                    }
                }
            }

            // Detect collision with boundaries
            if (_bubblePosition.X < 0 || _bubblePosition.X > GraphicsDevice.Viewport.Width - BubbleSize)
                _bubbleVelocity.X *= -1;
            if (_bubblePosition.Y < 0)
            {
                _bubblePosition = Vector2.Zero;
            }
        }

        // Ceiling dropping
        _dropTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_dropTimer >= _dropInterval)
        {
            DropCeiling();
            _dropTimer = 0;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw grid
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (_grid[row, col] != null)
                {
                    _spriteBatch.Draw(_grid[row, col].Texture, _grid[row, col].Position, Color.White);
                }
            }
        }

        // Draw launcher
        _spriteBatch.Draw(_launcherTexture, _launcherPosition, Color.White);

        // Draw shooting bubble
        if (_bubblePosition != Vector2.Zero)
        {
            _spriteBatch.Draw(_bubbleTexture, _bubblePosition, Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void PlaceBubble(int row, int col)
    {
        _grid[row, col] = new Tile(_bubbleTexture, new Vector2(col * BubbleSize, row * BubbleSize), _random.Next(3));
    }

    private void CheckMatches(int row, int col)
    {
        List<(int, int)> matches = new List<(int, int)>();
        FloodFill(row, col, _grid[row, col].Color, matches);

        if (matches.Count >= 3)
        {
            foreach (var (r, c) in matches)
            {
                _grid[r, c] = null;
            }
        }
    }

    private void FloodFill(int row, int col, int color, List<(int, int)> matches)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns || _grid[row, col] == null || _grid[row, col].Color != color || matches.Contains((row, col)))
            return;

        matches.Add((row, col));

        FloodFill(row - 1, col, color, matches);
        FloodFill(row + 1, col, color, matches);
        FloodFill(row, col - 1, color, matches);
        FloodFill(row, col + 1, color, matches);
    }

    private void DropCeiling()
    {
        for (int row = Rows - 1; row > 0; row--)
        {
            for (int col = 0; col < Columns; col++)
            {
                _grid[row, col] = _grid[row - 1, col];
                if (_grid[row, col] != null)
                {
                    _grid[row, col].Position = new Vector2(col * BubbleSize, row * BubbleSize);
                }
            }
        }

        for (int col = 0; col < Columns; col++)
        {
            _grid[0, col] = null;
        }
    }
}

public class Tile
{
    public Texture2D Texture { get; }
    public Vector2 Position { get; set; }
    public int Color { get; }

    public Tile(Texture2D texture, Vector2 position, int color)
    {
        Texture = texture;
        Position = position;
        Color = color;
    }
}
