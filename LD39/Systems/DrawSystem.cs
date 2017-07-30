using Artemis;
using Artemis.System;
using LD39.Components;
using LD39.Extensions;
using LD39.Tiles;
using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace LD39.Systems
{
    internal enum Layer
    {
        Floor,
        Below,
        Player,
        Above,
    }

    internal sealed class DrawSystem : EntityProcessingSystem
    {
        private readonly RenderTarget _target;
        private readonly TileMap _background, _foreground;
        private readonly Dictionary<Layer, List<Entity>> _sortedEntities = new Dictionary<Layer, List<Entity>>();

        public DrawSystem(RenderTarget target, TileMap background, TileMap foreground)
            : base(Aspect.All(typeof(PositionComponent), typeof(SpriteComponent)))
        {
            _target = target;
            _background = background;
            _foreground = foreground;

            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
                _sortedEntities.Add(layer, new List<Entity>());
        }

        public RenderStates RenderStates { get; set; }

        protected override void Begin()
        {
            base.Begin();

            _target.Draw(_background, RenderStates);

            foreach (List<Entity> entities in _sortedEntities.Values)
                entities.Clear();
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();

            for (int i = 0; i < _sortedEntities[spriteComponent.Layer].Count; i++)
                if (positionComponent.Position.Y < _sortedEntities[spriteComponent.Layer][i].GetComponent<PositionComponent>().Position.Y)
                {
                    _sortedEntities[spriteComponent.Layer].Insert(i, entity);
                    return;
                }

            _sortedEntities[spriteComponent.Layer].Add(entity);
        }

        protected override void End()
        {
            base.End();

            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
                foreach (Entity entity in _sortedEntities[layer])
                {
                    PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
                    SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();

                    RenderStates states = RenderStates;
                    states.Transform.Translate(positionComponent.Position.Floor());

                    _target.Draw(spriteComponent.Sprite, states);
                }

            _target.Draw(_foreground, RenderStates);
        }
    }
}
