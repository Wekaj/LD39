using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class PositionComponent : IComponent
    {
        public PositionComponent()
        {
            Position = new Vector2f();
        }

        public PositionComponent(Vector2f position)
        {
            Position = position;
        }

        public PositionComponent(float x, float y)
        {
            Position = new Vector2f(x, y);
        }

        public Vector2f Position { get; set; }
    }
}
