using Microsoft.Xna.Framework;

namespace IsometricGame.Classes.Particles
{
    public struct Particle
    {
        public Vector2 Position;        public Vector2 Origin;
        public Vector2 Velocity;
        public Color Color;
        public float MaxRange;

        public Particle(Vector2 position, Vector2 origin, Vector2 velocity, Color color, float maxRange)
        {
            Position = position;
            Origin = origin;
            Velocity = velocity;
            Color = color;
            MaxRange = maxRange;
        }
    }
}