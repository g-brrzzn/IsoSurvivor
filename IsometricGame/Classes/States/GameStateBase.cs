using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsometricGame.States
{
    public abstract class GameStateBase
    {
        public bool IsDone { get; protected set; }
        public string NextState { get; protected set; }

        public virtual void Start()
        {
            IsDone = false;
            NextState = string.Empty;
        }

        public abstract void Update(GameTime gameTime, InputManager input);

        public abstract void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);
    }
}