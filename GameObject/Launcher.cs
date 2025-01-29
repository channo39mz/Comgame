using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Comgame.GameObject;

class Launcher : GameObject
{
    private float _rotation;
    private Vector2 _position;
	private MovingBubble _loadedBubble;
	private MovingBubble _nextBubble;

    public Launcher(Texture2D texture, Vector2 position) : base(texture)
    {
        _position = position;
        _rotation = 0f;
		_loadedBubble = GenerateRandomBubble();
        _nextBubble = GenerateRandomBubble();
    }

	private MovingBubble GenerateRandomBubble()
    {
        return new MovingBubble(new Vector2(Singleton.GAMEWIDTH * Singleton.TILESIZE + Singleton.SCOREWIDTH * Singleton.TILESIZE / 2, Singleton.TILESIZE));
    }

    public override void Update(GameTime gameTime)
    {
        var keyboardState = Singleton.Instance.CurrentKey;

        if (keyboardState.IsKeyDown(Keys.Left))
            _rotation -= 0.05f;
        if (keyboardState.IsKeyDown(Keys.Right))
            _rotation += 0.05f;
		
		if (_rotation >= 1.3f)
			_rotation = 1.3f;
		if (_rotation <= -1.3f)
			_rotation = -1.3f;

		_loadedBubble.Position = _position - new Vector2(Singleton.TILESIZE / 2, Singleton.TILESIZE / 2 - 10);

		base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, _position, null, Color.White, _rotation,
            new Vector2(_texture.Width / 2, _texture.Height / 2), 1f, SpriteEffects.None, 0f);

		_loadedBubble.Draw(spriteBatch);
		_nextBubble.Draw(spriteBatch);

		base.Draw(spriteBatch);
    }
}