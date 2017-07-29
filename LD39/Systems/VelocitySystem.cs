using Artemis;
using LD39.Components;

namespace LD39.Systems
{
    internal sealed class VelocitySystem : EntityUpdatingSystem
    {
        public VelocitySystem() 
            : base(Aspect.All(typeof(PositionComponent), typeof(VelocityComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();

            positionComponent.Position += velocityComponent.Velocity * DeltaTime.AsSeconds();
        }
    }
}
