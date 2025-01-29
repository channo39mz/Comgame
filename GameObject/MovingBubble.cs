using Microsoft.Xna.Framework;

namespace Comgame.GameObject;

class MovingBubble : Bubble
{
    public Vector2 Velocity;

    public MovingBubble(Vector2 position) : base(position)
    {
        Velocity = new Vector2(1.0f, 1.0f);
    }

    public override void Update(GameTime gameTime)
    {
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
		base.Update(gameTime);
    }
}