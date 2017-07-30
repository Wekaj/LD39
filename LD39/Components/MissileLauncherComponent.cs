using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class MissileLauncherComponent : IComponent
    {
        public MissileLauncherComponent(Time timer)
        {
            Timer = timer;
        }

        public Time Timer { get; set; }
    }
}
