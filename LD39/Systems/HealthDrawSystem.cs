using Artemis;
using Artemis.System;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using SFML.Graphics;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class HealthDrawSystem : EntityProcessingSystem
    {
        private readonly RenderTarget _target;
        private readonly Sprite _healthBack, _healthFill;

        public HealthDrawSystem(RenderTarget target, TextureLoader textures)
            : base(Aspect.All(typeof(PositionComponent), typeof(HealthComponent)))
        {
            _target = target;
            _healthBack = new Sprite(textures[TextureID.HealthBack]);
            _healthFill = new Sprite(textures[TextureID.HealthFill]);
        }

        public RenderStates RenderStates { get; set; }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            HealthComponent healthComponent = entity.GetComponent<HealthComponent>();

            if (healthComponent.Health <= 0)
                entity.Delete();
            if (healthComponent.Health == healthComponent.MaxHealth)
                return;

            _healthBack.Position = (positionComponent.Position - new Vector2f(_healthBack.Texture.Size.X / 2f, healthComponent.Height)).Floor();
            _healthFill.Position = _healthBack.Position + new Vector2f(1f, 1f);
            _healthFill.TextureRect = new IntRect(0, 
                0, 
                (int)(((float)healthComponent.Health / healthComponent.MaxHealth) * _healthFill.Texture.Size.X), 
                (int)_healthFill.Texture.Size.Y);

            _target.Draw(_healthBack, RenderStates);
            _target.Draw(_healthFill, RenderStates);
        }
    }
}
