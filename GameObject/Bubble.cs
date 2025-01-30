using System;
using System.Collections.Generic;
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
	public BubbleColor CurrentColor { get; private set; }
	private static Dictionary<BubbleColor, Texture2D> _bubbleTextures;

	public Bubble(Vector2 position , BubbleColor color)
	{
		Position = position;
		CurrentColor = color;
		_texture = _bubbleTextures[CurrentColor];
	}

	public Bubble(Vector2 position)
	{
		Position = position;
		CurrentColor = GetRandomColor();
		_texture = _bubbleTextures[CurrentColor];
	}

	public static void LoadTextures(Dictionary<BubbleColor, Texture2D> textures)
	{
		_bubbleTextures = textures;
	}

	private static BubbleColor GetRandomColor()
	{
		return (BubbleColor)Singleton.Instance.Random.Next(Enum.GetValues(typeof(BubbleColor)).Length);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		var drawObj = new Rectangle((int)Position.X, (int)Position.Y, Singleton.TILESIZE, Singleton.TILESIZE);
		if (_texture != null)
            {
                spriteBatch.Draw(_texture, drawObj, Color.White);
            }

		base.Draw(spriteBatch);
	}

}
