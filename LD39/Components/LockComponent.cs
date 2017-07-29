using Artemis;
using Artemis.Interface;

namespace LD39.Components
{
    internal sealed class LockComponent : IComponent
    {
        public LockComponent(Entity parent)
        {
            Parent = parent;
        }

        public Entity Parent { get; set; }
    }
}
