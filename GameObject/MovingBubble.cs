using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Comgame.GameObject;

class MovingBubble : Bubble
{
    public Vector2 Velocity;

    public MovingBubble(Texture2D texture, Vector2 position) : base(texture, position)
    {
        Velocity = new Vector2(1.0f, 1.0f);
    }

    public override void Update(GameTime gameTime)
    {
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
		base.Update(gameTime);
    }
}