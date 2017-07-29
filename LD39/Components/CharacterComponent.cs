using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class CharacterComponent : IComponent
    {
        public Vector2f LastVelocity { get; set; }
    }
}
