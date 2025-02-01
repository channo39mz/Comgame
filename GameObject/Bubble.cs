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
		GREEN,
		BLACKHOLE // always put blackhole at the end
	}
	public BubbleColor CurrentColor { get; protected set; }
	protected static Dictionary<BubbleColor, Texture2D> _bubbleTextures;

	public Bubble(Vector2 position , BubbleColor color)
	{
		Position = position;
		CurrentColor = color;
		_texture = _bubbleTextures[CurrentColor];
	}

	public Bubble(Vector2 position)
	{
		Position = position;
		CurrentColor = Singleton.RandomByPercent(2) ? BubbleColor.BLACKHOLE : GetRandomColor();
		_texture = _bubbleTextures[CurrentColor];
	}

	public static void LoadTextures(Dictionary<BubbleColor, Texture2D> textures)
	{
		_bubbleTextures = textures;
	}

	protected static BubbleColor GetRandomColor()
	{
		return (BubbleColor)Singleton.Instance.Random.Next(Enum.GetValues(typeof(BubbleColor)).Length - 1); // exclude blackhole
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
