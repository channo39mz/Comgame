﻿using Comgame.GameObject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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
            Exit();



        _launcher.Update(gameTime);

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
        _spriteBatch.DrawString(_font, "Next:", new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 4, Singleton.TILESIZE), Color.White);

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    protected void Reset()
    {
        _launcher = new Launcher(_launcherTexture, new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE / 2, (Singleton.GAMEHEIGHT + 1) * Singleton.TILESIZE));
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
}
