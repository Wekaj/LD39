using Artemis;
using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class LockComponent : IComponent
    {
        public LockComponent(Entity parent, Vector2f offset)
        {
            Parent = parent;
            Offset = offset;
        }

        public Entity Parent { get; set; }
        public Vector2f Offset { get; set; }
    }
}
