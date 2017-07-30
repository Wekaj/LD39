using Artemis;
using LD39.Components;
using System.Collections.Generic;

namespace LD39.Systems
{
    internal sealed class HitSystem : EntityUpdatingSystem
    {
        private readonly List<Entity> _removing = new List<Entity>();

        public HitSystem() 
            : base(Aspect.All(typeof(HitComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            HitComponent hitComponent = entity.GetComponent<HitComponent>();

            foreach (Entity source in hitComponent.HitSources)
                if (source.DeletingState)
                    _removing.Add(source);

            for (int i = 0; i < _removing.Count; i++)
                hitComponent.HitSources.Remove(_removing[i]);

            _removing.Clear();
        }
    }
}
