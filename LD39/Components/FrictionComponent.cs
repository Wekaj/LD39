using Artemis.Interface;

namespace LD39.Components
{
    internal sealed class FrictionComponent : IComponent
    {
        public FrictionComponent(float friction)
        {
            Friction = friction;
        }

        public float Friction { get; set; }
    }
}
