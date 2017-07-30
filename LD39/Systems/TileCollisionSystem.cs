using Artemis;
using LD39.Components;
using LD39.Resources;
using SFML.Audio;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class TileCollisionSystem : EntityUpdatingSystem
    {
        private readonly bool[,] _collisions;
        private readonly float _tileSize;
        private readonly Sound _thudSound;
        private readonly Time _thudTime = Time.FromSeconds(0.4f);
        private Time _thudTimer;

        public TileCollisionSystem(bool[,] collisions, float tileSize, SoundBufferLoader soundBuffers) 
            : base(Aspect.All(typeof(PositionComponent), typeof(VelocityComponent), typeof(TileCollisionComponent)))
        {
            _collisions = collisions;
            _tileSize = tileSize;
            _thudSound = new Sound(soundBuffers[SoundBufferID.Thud]);
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            TileCollisionComponent tileCollisionComponent = entity.GetComponent<TileCollisionComponent>();

            bool collision = false;
            if (velocityComponent.Velocity.X > 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    + new Vector2f(tileCollisionComponent.Size, 0f));
                if (GetCollision(futureTile))
                {
                    positionComponent.Position = new Vector2f(futureTile.X * _tileSize - tileCollisionComponent.Size, positionComponent.Position.Y);
                    velocityComponent.Velocity = new Vector2f(0f, velocityComponent.Velocity.Y);
                    collision = true;
                }
            }
            else if (velocityComponent.Velocity.X < 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    - new Vector2f(tileCollisionComponent.Size, 0f));
                if (GetCollision(futureTile))
                {
                    positionComponent.Position = new Vector2f((futureTile.X + 1f) * _tileSize + tileCollisionComponent.Size, positionComponent.Position.Y);
                    velocityComponent.Velocity = new Vector2f(0f, velocityComponent.Velocity.Y);
                    collision = true;
                }
            }

            if (velocityComponent.Velocity.Y > 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    + new Vector2f(0f, tileCollisionComponent.Size));
                if (GetCollision(futureTile))
                {
                    positionComponent.Position = new Vector2f(positionComponent.Position.X, futureTile.Y * _tileSize - tileCollisionComponent.Size);
                    velocityComponent.Velocity = new Vector2f(velocityComponent.Velocity.X, 0f);
                    collision = true;
                }
            }
            else if (velocityComponent.Velocity.Y < 0f)
            {
                Vector2i futureTile = GetTile(positionComponent.Position + velocityComponent.Velocity * DeltaTime.AsSeconds()
                    - new Vector2f(0f, tileCollisionComponent.Size));
                if (GetCollision(futureTile))
                {
                    positionComponent.Position = new Vector2f(positionComponent.Position.X, (futureTile.Y + 1f) * _tileSize + tileCollisionComponent.Size);
                    velocityComponent.Velocity = new Vector2f(velocityComponent.Velocity.X, 0f);
                    collision = true;
                }
            }

            if (entity.HasComponent<CharacterComponent>())
            {
                if (_thudTimer > DeltaTime)
                    _thudTimer -= DeltaTime;
                else
                    _thudTimer = Time.Zero;

                if (collision && _thudTimer <= Time.Zero)
                {
                    _thudSound.Play();
                    _thudTimer += _thudTime;
                }
            }
        }

        private Vector2i GetTile(Vector2f position)
        {
            return new Vector2i((int)(position.X / _tileSize), (int)(position.Y / _tileSize));
        }

        private bool GetCollision(Vector2i tile)
        {
            if (tile.X < 0 || tile.Y < 0 || tile.X >= _collisions.GetLength(0) || tile.Y >= _collisions.GetLength(1))
                return false;
            return _collisions[tile.X, tile.Y];
        }

        private bool GetCollision(Vector2f position)
        {
            return GetCollision(GetTile(position));
        }
    }
}
