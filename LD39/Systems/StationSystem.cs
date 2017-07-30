using Artemis;
using LD39.Components;
using LD39.Extensions;
using SFML.System;
using System;
using System.Linq;

namespace LD39.Systems
{
    internal sealed class StationSystem : EntityUpdatingSystem
    {
        private const float _radius = 4f;
        private Entity _character;

        public StationSystem() 
            : base(Aspect.All(typeof(PositionComponent), typeof(StationComponent)))
        {
        }

        public event EventHandler<StationEventArgs> StationTouched;

        protected override void Begin()
        {
            base.Begin();

            _character = EntityWorld.EntityManager.GetEntities(Aspect.All(typeof(CharacterComponent))).First();
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            StationComponent stationComponent = entity.GetComponent<StationComponent>();

            PositionComponent playerPositionComponent = _character.GetComponent<PositionComponent>();
            TileCollisionComponent playerCollisionComponent = _character.GetComponent<TileCollisionComponent>();

            Vector2f difference = positionComponent.Position - playerPositionComponent.Position;

            if (difference.GetLength() < _radius + playerCollisionComponent.Size)
            {
                if (!stationComponent.Colliding)
                    StationTouched?.Invoke(this, new StationEventArgs(stationComponent.ID));

                stationComponent.Colliding = true;
            }
            else
                stationComponent.Colliding = false;
        }
    }
}
