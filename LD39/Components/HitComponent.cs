using Artemis;
using Artemis.Interface;
using System.Collections.Generic;

namespace LD39.Components
{
    internal sealed class HitComponent : IComponent
    {
        public HashSet<Entity> HitSources { get; } = new HashSet<Entity>();
    }
}
