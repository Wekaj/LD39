using Artemis.Interface;
using LD39.Systems;
using SFML.Graphics;

namespace LD39.Components
{
    internal sealed class SpriteComponent : IComponent
    {
        public SpriteComponent(Sprite sprite, Layer layer)
        {
            Sprite = sprite;
            Layer = layer;
        }

        public Sprite Sprite { get; set; }
        public Layer Layer { get; set; }
    }
}
