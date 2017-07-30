using Artemis;
using System;

namespace LD39.Components
{
    internal class EntityEventArgs : EventArgs
    {
        public EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }

        public Entity Entity { get; }
    }
}
