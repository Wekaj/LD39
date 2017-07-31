using Artemis;
using Artemis.System;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class HealthDrawSystem : EntityProcessingSystem
    {
        private readonly RenderTarget _target;
        private readonly Sprite _healthBack, _healthFill;
        private readonly Texture _explosionTexture;
        private readonly FixedFrameAnimation _explosionAnimation;
        private readonly Sound _explosionSound;

        public HealthDrawSystem(RenderTarget target, TextureLoader textures, SoundBufferLoader soundBuffers)
            : base(Aspect.All(typeof(PositionComponent), typeof(HealthComponent)))
        {
            _target = target;
            _healthBack = new Sprite(textures[TextureID.HealthBack]);
            _healthFill = new Sprite(textures[TextureID.HealthFill]);
            _explosionTexture = textures[TextureID.Explosion];
            _explosionAnimation = new FixedFrameAnimation(32, 32);
            for (int i = 0; i < 10; i++)
                _explosionAnimation.AddFrame(i, 0, 1f);
            _explosionSound = new Sound(soundBuffers[SoundBufferID.Explosion]) { Volume = 30f };
        }

        public RenderStates RenderStates { get; set; }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            HealthComponent healthComponent = entity.GetComponent<HealthComponent>();

            if (healthComponent.Health <= 0)
            {
                _explosionSound.Play();

                Entity explosion = EntityWorld.CreateEntity();
                explosion.AddComponent(new PositionComponent(entity.GetComponent<PositionComponent>().Position));
                explosion.AddComponent(new SpriteComponent(new Sprite(_explosionTexture) { Position = new Vector2f(-16f, -20f) }, Layer.Above));
                explosion.AddComponent(new AnimationComponent() { DestroyAtEnd = true });
                explosion.GetComponent<AnimationComponent>().Play(_explosionAnimation, Time.FromSeconds(0.5f));

                entity.Delete();
            }
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
