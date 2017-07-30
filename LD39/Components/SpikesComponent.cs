using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class SpikesComponent : IComponent
    {
        public SpikesComponent(Vector2i tile, Time timer)
        {
            Tile = tile;
            Timer = timer;
        }

        public bool Extended { get; set; } = false;
        public Vector2i Tile { get; set; }
        public Time Timer { get; set; }
    }
}
