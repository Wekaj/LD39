using Artemis;
using LD39.Animation;
using LD39.Components;
using LD39.Resources;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace LD39.Systems
{
    internal sealed class MissileLauncherSystem : EntityUpdatingSystem
    {
        private readonly Time _missileTime = Time.FromSeconds(4f);
        private readonly Texture _missileTexture, _explosionTexture;
        private readonly FixedFrameAnimation _missileAnimation, _explosionAnimation;
        private readonly Sound _explosionSound;
        private readonly List<Entity> _missiles = new List<Entity>();

        public MissileLauncherSystem(TextureLoader textures, SoundBufferLoader soundBuffers) 
            : base(Aspect.All(typeof(PositionComponent), typeof(MissileLauncherComponent)))
        {
            _missileTexture = textures[TextureID.Missile];
            _explosionTexture = textures[TextureID.Explosion];
            _missileAnimation = new FixedFrameAnimation(10, 22).AddFrame(0, 0, 1f).AddFrame(1, 0, 1f);
            _explosionAnimation = new FixedFrameAnimation(32, 32);
            for (int i = 0; i < 10; i++)
                _explosionAnimation.AddFrame(i, 0, 1f);
            _explosionSound = new Sound(soundBuffers[SoundBufferID.Explosion]) { Volume = 30f };
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            MissileLauncherComponent missileLauncherComponent = entity.GetComponent<MissileLauncherComponent>();

            if (missileLauncherComponent.Timer > DeltaTime)
                missileLauncherComponent.Timer -= DeltaTime;
            else
            {
                missileLauncherComponent.Timer += _missileTime - DeltaTime;

                Entity missile = EntityWorld.CreateEntity();
                missile.AddComponent(new PositionComponent(positionComponent.Position));
                missile.AddComponent(new VelocityComponent(0f, 70f));

                missile.AddComponent(new CollisionComponent(4f, false));
                missile.GetComponent<CollisionComponent>().Collided += (sender, e) => Missile_Collided(missile, e.Entity);

                missile.AddComponent(new SpriteComponent(new Sprite(_missileTexture) { Position = new Vector2f(-5f, -16f) }, Layer.Player));

                missile.AddComponent(new AnimationComponent());
                missile.GetComponent<AnimationComponent>().Play(_missileAnimation, Time.FromSeconds(0.25f), true);

                _missiles.Add(missile);
            }
        }

        protected override void End()
        {
            base.End();

            for (int i = 0; i < _missiles.Count; i++)
            {
                if (_missiles[i].DeletingState || !_missiles[i].HasComponent<PositionComponent>())
                {
                    _missiles.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_missiles[i].GetComponent<PositionComponent>().Position.Y > 128f * 16f)
                    _missiles[i].Delete();
                if (_missiles[i].DeletingState)
                {
                    _missiles.RemoveAt(i);
                    i--;
                }
            }
        }

        private void Missile_Collided(Entity missile, Entity entity)
        {
            if (!entity.GetComponent<CollisionComponent>().Solid)
                return;
            if (!entity.HasComponent<HitComponent>())
                return;

            HitComponent hitComponent = entity.GetComponent<HitComponent>();
            if (hitComponent.HitSources.Contains(missile))
                return;
            hitComponent.HitSources.Add(missile);

            if (entity.HasComponent<CharacterComponent>())
                entity.GetComponent<CharacterComponent>().Power -= 3f / 7f;
            
            if (entity.HasComponent<HealthComponent>())
                entity.GetComponent<HealthComponent>().Health -= 4;

            if (entity.HasComponent<VelocityComponent>())
                entity.GetComponent<VelocityComponent>().Velocity += new Vector2f(0f, 50f);

            _explosionSound.Play();

            missile.Delete();

            Entity explosion = EntityWorld.CreateEntity();
            explosion.AddComponent(new PositionComponent(missile.GetComponent<PositionComponent>().Position));
            explosion.AddComponent(new SpriteComponent(new Sprite(_explosionTexture) { Position = new Vector2f(-16f, -16f) }, Layer.Above));
            explosion.AddComponent(new AnimationComponent() { DestroyAtEnd = true });
            explosion.GetComponent<AnimationComponent>().Play(_explosionAnimation, Time.FromSeconds(0.5f));
        }
    }
}
