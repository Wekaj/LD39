using Artemis;
using Artemis.Interface;
using SFML.System;
using System;

namespace LD39.Components
{
    internal sealed class CollisionComponent : IComponent
    {
        public CollisionComponent(float radius, bool solid = true)
        {
            Radius = radius;
            Solid = solid;
        }

        public event EventHandler<EntityEventArgs> Collided;

        public float Radius { get; set; }
        public bool Solid { get; set; }
        public Entity Ignore { get; set; }
        public Time Timer { get; set; }
        public bool Temporary { get; set; } = false;

        public void OnCollided(Entity entity)
        {
            Collided?.Invoke(this, new EntityEventArgs(entity));
        }
    }
}
