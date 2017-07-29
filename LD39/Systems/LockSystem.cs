using Artemis;
using Artemis.System;
using LD39.Components;

namespace LD39.Systems
{
    internal sealed class LockSystem : EntityProcessingSystem
    {
        public LockSystem() 
            : base(Aspect.All(typeof(PositionComponent), typeof(LockComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            LockComponent lockComponent = entity.GetComponent<LockComponent>();

            positionComponent.Position = lockComponent.Parent.GetComponent<PositionComponent>().Position;
        }
    }
}
