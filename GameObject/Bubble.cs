using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Comgame.GameObject;

class Bubble : GameObject
{
    public enum BubbleColor
    {
        RED,
        YELLOW,
        BLUE,
        GREEN
    }
    private BubbleColor _color;

    public Bubble(Texture2D texture, Vector2 position) : base(texture)
    {
        Position = position;
        _color = (BubbleColor)Singleton.Instance.Random.Next(0, 4);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
		var drawObj = new Rectangle((int)Position.X, (int)Position.Y, Singleton.TILESIZE, Singleton.TILESIZE);
		switch (_color)
		{
			case BubbleColor.RED:
        		spriteBatch.Draw(_texture, drawObj, Color.Red);
				break;
			case BubbleColor.YELLOW:
        		spriteBatch.Draw(_texture, drawObj, Color.Yellow);
				break;
			case BubbleColor.BLUE:
        		spriteBatch.Draw(_texture, drawObj, Color.Blue);
				break;
			case BubbleColor.GREEN:
        		spriteBatch.Draw(_texture, drawObj, Color.Green);
				break;
		}

		base.Draw(spriteBatch);
    }
}
