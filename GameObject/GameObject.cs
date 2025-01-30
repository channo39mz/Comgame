using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Comgame.GameObject;
class GameObject
{
	protected Texture2D _texture;

	public Vector2 Position;
	public float Rotation;
	public Vector2 Scale;

	public string Name;

	public Rectangle Rectangle
	{
		get
		{
			return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
		}
	}

	public GameObject(Texture2D texture)
	{
		_texture = texture;
		Position = Vector2.Zero;
		Scale = Vector2.One;
		Rotation = 0f;
	}
	public GameObject()
	{
		Position = Vector2.Zero;
		Scale = Vector2.One;
		Rotation = 0f;
	}


	public virtual void Update(GameTime gameTime)
	{
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
	}

	public virtual void Reset()
	{

	}
}
