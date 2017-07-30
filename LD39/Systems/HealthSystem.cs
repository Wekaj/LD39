using Artemis;
using LD39.Components;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class HealthSystem : EntityUpdatingSystem
    {
        public HealthSystem() 
            : base(Aspect.All(typeof(HealthComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            HealthComponent healthComponent = entity.GetComponent<HealthComponent>();

            if (healthComponent.HitTimer > DeltaTime)
                healthComponent.HitTimer -= DeltaTime;
            else
                healthComponent.HitTimer = Time.Zero;
        }
    }
}
