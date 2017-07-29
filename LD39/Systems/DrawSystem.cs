using Artemis;
using Artemis.System;
using LD39.Components;
using LD39.Tiles;
using SFML.Graphics;
using System.Collections.Generic;

namespace LD39.Systems
{
    internal sealed class DrawSystem : EntityProcessingSystem
    {
        private readonly RenderTarget _target;
        private readonly TileMap _background, _foreground;
        private readonly List<Entity> _sortedEntities = new List<Entity>();

        public DrawSystem(RenderTarget target, TileMap background, TileMap foreground)
            : base(Aspect.All(typeof(PositionComponent), typeof(SpriteComponent)))
        {
            _target = target;
            _background = background;
            _foreground = foreground;
        }

        public RenderStates RenderStates { get; set; }

        protected override void Begin()
        {
            base.Begin();

            _target.Draw(_background, RenderStates);

            _sortedEntities.Clear();
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            for (int i = 0; i < _sortedEntities.Count; i++)
                if (positionComponent.Position.Y < _sortedEntities[i].GetComponent<PositionComponent>().Position.Y)
                {
                    _sortedEntities.Insert(i, entity);
                    return;
                }

            _sortedEntities.Add(entity);
        }

        protected override void End()
        {
            base.End();

            foreach (Entity entity in _sortedEntities)
            {
                PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
                SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();

                RenderStates states = RenderStates;
                states.Transform.Translate(positionComponent.Position);

                _target.Draw(spriteComponent.Sprite, states);
            }

            _target.Draw(_foreground, RenderStates);
        }
    }
}
