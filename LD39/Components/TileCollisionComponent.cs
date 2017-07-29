using Artemis.Interface;

namespace LD39.Components
{
    internal sealed class TileCollisionComponent : IComponent
    {
        public TileCollisionComponent(float size)
        {
            Size = size;
        }

        public float Size { get; set; }
    }
}
