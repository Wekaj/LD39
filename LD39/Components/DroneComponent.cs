using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class DroneComponent : IComponent
    {
        public Time PulseTimer { get; set; }
    }
}
