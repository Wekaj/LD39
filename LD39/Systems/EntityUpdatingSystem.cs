using Artemis;
using Artemis.System;
using SFML.System;

namespace LD39.Systems
{
    internal abstract class EntityUpdatingSystem : EntityProcessingSystem
    {
        protected EntityUpdatingSystem(Aspect aspect)
            : base(aspect)
        {
        }

        protected Time DeltaTime { get; private set; }

        protected override void Begin()
        {
            base.Begin();

            DeltaTime = Time.FromMicroseconds(EntityWorld.Delta / 10);
        }
    }
}
