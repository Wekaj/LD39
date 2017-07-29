using Artemis.Interface;
using SFML.Graphics;

namespace LD39.Components
{
    internal sealed class SpriteComponent : IComponent
    {
        public SpriteComponent(Sprite sprite)
        {
            Sprite = sprite;
        }

        public Sprite Sprite { get; set; }
    }
}
