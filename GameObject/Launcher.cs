using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Comgame.GameObject;

class Launcher : GameObject
{
    private float _rotation;
    private Vector2 _position;
    private List<MovingBubble> _movingBubbles;

    private MainScene _mainScene;

    public Launcher(Texture2D texture, Vector2 position, MainScene mainScene) : base(texture)
    {
        _position = position;
        _rotation = 0f;
        _movingBubbles = new List<MovingBubble>();
        _mainScene = mainScene;

        Singleton.Instance.CurrentBubble = GenerateRandomBubble();
        Singleton.Instance.NextBubble = GenerateRandomBubble();
    }

    private MovingBubble GenerateRandomBubble()
    {
        return new MovingBubble(new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 2, Singleton.TILESIZE), _mainScene);
    }

    private void ShootBubble()
    {
        if (Singleton.Instance.CurrentBubble != null)
        {
            var bubble = Singleton.Instance.CurrentBubble;
            bubble.Velocity = new Vector2((float)Math.Sin(_rotation), -(float)Math.Cos(_rotation)) * 300f;
            _movingBubbles.Add(bubble);
            Singleton.Instance.CurrentBubble = Singleton.Instance.NextBubble;
            Singleton.Instance.NextBubble = GenerateRandomBubble();
        }
    }

    public override void Update(GameTime gameTime)
    {
        var keyboardState = Singleton.Instance.CurrentKey;

        if (keyboardState.IsKeyDown(Keys.Left))
            _rotation -= 0.05f;
        if (keyboardState.IsKeyDown(Keys.Right))
            _rotation += 0.05f;
        
        _rotation = MathHelper.Clamp(_rotation, -1.3f, 1.3f);

        if (keyboardState.IsKeyDown(Keys.Space) && Singleton.Instance.PreviousKey.IsKeyUp(Keys.Space))
        {
            ShootBubble();
        }

        foreach (var bubble in _movingBubbles)
        {
            bubble.Update(gameTime);
        }

        _movingBubbles.RemoveAll(b => b.HasStopped);

        Singleton.Instance.CurrentBubble.Position = _position - new Vector2(Singleton.TILESIZE / 2, Singleton.TILESIZE / 2 - 10);
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, _position, null, Color.White, _rotation,
            new Vector2(_texture.Width / 2, _texture.Height / 2), 1f, SpriteEffects.None, 0f);

        Singleton.Instance.CurrentBubble.Draw(spriteBatch);
        Singleton.Instance.NextBubble.Draw(spriteBatch);

        foreach (var bubble in _movingBubbles)
        {
            bubble.Draw(spriteBatch);
        }

        base.Draw(spriteBatch);
    }
}
