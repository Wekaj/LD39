using Artemis;
using LD39.Components;
using LD39.Extensions;
using SFML.System;
using System.Collections.Generic;

namespace LD39.Systems
{
    internal sealed class CollisionSystem : EntityUpdatingSystem
    {
        private readonly List<Entity> _entities = new List<Entity>();

        public CollisionSystem() 
            : base(Aspect.All(typeof(PositionComponent), typeof(CollisionComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            _entities.Add(entity);
        }

        protected override void End()
        {
            base.End();

            for (int i = 0; i < _entities.Count; i++)
                for (int j = i + 1; j < _entities.Count; j++)
                    DoCollision(_entities[i], _entities[j]);
            _entities.Clear();
        }

        private void DoCollision(Entity entity1, Entity entity2)
        {
            PositionComponent positionComponent1 = entity1.GetComponent<PositionComponent>(),
                positionComponent2 = entity2.GetComponent<PositionComponent>();
            CollisionComponent collisionComponent1 = entity1.GetComponent<CollisionComponent>(),
                collisionComponent2 = entity2.GetComponent<CollisionComponent>();

            if (entity1 == collisionComponent2.Ignore || entity2 == collisionComponent1.Ignore)
                return;

            Vector2f difference = positionComponent1.Position - positionComponent2.Position;
            float gap = difference.GetLength() - collisionComponent1.Radius - collisionComponent2.Radius;

            if (gap < 0f)
            {
                Vector2f position1 = positionComponent1.Position,
                    position2 = positionComponent2.Position;

                if (collisionComponent1.Solid && collisionComponent2.Solid)
                {
                    positionComponent1.Position = position2 + difference.Normalize() * (collisionComponent1.Radius + collisionComponent2.Radius);
                    positionComponent2.Position = position1 + -difference.Normalize() * (collisionComponent1.Radius + collisionComponent2.Radius);
                }

                collisionComponent1.OnCollided(entity2);
                collisionComponent2.OnCollided(entity1);
            }
        }
    }
}
