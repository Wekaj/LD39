using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class VelocityComponent : IComponent
    {
        public VelocityComponent()
        {
            Velocity = new Vector2f();
        }

        public VelocityComponent(Vector2f velocity)
        {
            Velocity = velocity;
        }

        public VelocityComponent(float x, float y)
        {
            Velocity = new Vector2f(x, y);
        }

        public Vector2f Velocity { get; set; }
    }
}
