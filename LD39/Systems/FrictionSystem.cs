using Artemis;
using LD39.Components;
using LD39.Extensions;

namespace LD39.Systems
{
    internal sealed class FrictionSystem : EntityUpdatingSystem
    {
        public FrictionSystem() 
            : base(Aspect.All(typeof(VelocityComponent), typeof(FrictionComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            FrictionComponent frictionComponent = entity.GetComponent<FrictionComponent>();

            float velocity = velocityComponent.Velocity.GetLength();
            if (velocity > frictionComponent.Friction * DeltaTime.AsSeconds())
                velocity -= frictionComponent.Friction * DeltaTime.AsSeconds();
            else
                velocity = 0f;

            velocityComponent.Velocity = velocityComponent.Velocity.Normalize() * velocity;
        }
    }
}
