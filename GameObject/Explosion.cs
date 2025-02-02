using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Comgame.GameObject
{
    public class Explosion
    {
        private Texture2D _texture;
        private Vector2 _position;
        private int _frameWidth;
        private int _frameHeight;
        private int _totalFrames;
        private int _currentFrame;
        private double _timePerFrame;
        private double _elapsedTime;
        private bool _loop;

        public bool IsFinished { get; private set; }

        public Explosion(Texture2D texture, Vector2 bubbleCenter, int frameWidth, int frameHeight, int columns, int rows, double frameTime, bool loop = false)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;

            // Calculate the position to center the explosion on the bubble
            _position = new Vector2(
                bubbleCenter.X - (_frameWidth / 2),
                bubbleCenter.Y - (_frameHeight / 2)
            );

            _totalFrames = columns * rows;
            _currentFrame = 0;
            _timePerFrame = frameTime;
            _loop = loop;
            IsFinished = false;
        }

        public void Update(GameTime gameTime)
        {
            if (IsFinished) return;

            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime >= _timePerFrame)
            {
                _currentFrame++;
                _elapsedTime = 0;

                if (_currentFrame >= _totalFrames)
                {
                    if (_loop) _currentFrame = 0;
                    else IsFinished = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsFinished) return;

            int row = (int)((float)_currentFrame / (_texture.Width / _frameWidth));
            int column = _currentFrame % (_texture.Width / _frameWidth);

            Rectangle sourceRect = new Rectangle(_frameWidth * column, _frameHeight * row, _frameWidth, _frameHeight);
            Rectangle destRect = new Rectangle((int)_position.X, (int)_position.Y, _frameWidth, _frameHeight);

            spriteBatch.Draw(_texture, destRect, sourceRect, Color.White);
        }
    }
}