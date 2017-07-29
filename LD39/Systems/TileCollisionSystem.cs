using Artemis;
using LD39.Components;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class TileCollisionSystem : EntityUpdatingSystem
    {
        private readonly bool[,] _collisions;
        private readonly float _tileSize;

        public TileCollisionSystem(bool[,] collisions, float tileSize) 
            : base(Aspect.All(typeof(PositionComponent), typeof(VelocityComponent), typeof(TileCollisionComponent)))
        {
            _collisions = collisions;
            _tileSize = tileSize;
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            TileCollisionComponent tileCollisionComponent = entity.GetComponent<TileCollisionComponent>();

            if (velocityComponent.Velocity.X > 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    + new Vector2f(tileCollisionComponent.Size, 0f));
                if (_collisions[futureTile.X, futureTile.Y])
                {
                    positionComponent.Position = new Vector2f(futureTile.X * _tileSize - tileCollisionComponent.Size, positionComponent.Position.Y);
                    velocityComponent.Velocity = new Vector2f(0f, velocityComponent.Velocity.Y);
                }
            }
            else if (velocityComponent.Velocity.X < 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    - new Vector2f(tileCollisionComponent.Size, 0f));
                if (_collisions[futureTile.X, futureTile.Y])
                {
                    positionComponent.Position = new Vector2f((futureTile.X + 1f) * _tileSize + tileCollisionComponent.Size, positionComponent.Position.Y);
                    velocityComponent.Velocity = new Vector2f(0f, velocityComponent.Velocity.Y);
                }
            }

            if (velocityComponent.Velocity.Y > 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    + new Vector2f(0f, tileCollisionComponent.Size));
                if (_collisions[futureTile.X, futureTile.Y])
                {
                    positionComponent.Position = new Vector2f(positionComponent.Position.X, futureTile.Y * _tileSize - tileCollisionComponent.Size);
                    velocityComponent.Velocity = new Vector2f(velocityComponent.Velocity.X, 0f);
                }
            }
            else if (velocityComponent.Velocity.Y < 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    - new Vector2f(0f, tileCollisionComponent.Size));
                if (_collisions[futureTile.X, futureTile.Y])
                {
                    positionComponent.Position = new Vector2f(positionComponent.Position.X, (futureTile.Y + 1f) * _tileSize + tileCollisionComponent.Size);
                    velocityComponent.Velocity = new Vector2f(velocityComponent.Velocity.X, 0f);
                }
            }
        }

        private Vector2i GetTile(Vector2f position)
        {
            return new Vector2i((int)(position.X / _tileSize), (int)(position.Y / _tileSize));
        }

        private bool GetCollision(Vector2f position)
        {
            return _collisions[(int)(position.X / _tileSize), (int)(position.Y / _tileSize)];
        }
    }
}
