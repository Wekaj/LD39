using Artemis;
using LD39.Animation;
using LD39.Components;
using LD39.Resources;
using SFML.Graphics;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class MissileLauncherSystem : EntityUpdatingSystem
    {
        private readonly Time _missileTime = Time.FromSeconds(4f);
        private readonly Texture _missileTexture;
        private readonly FixedFrameAnimation _missileAnimation;

        public MissileLauncherSystem(TextureLoader textures) 
            : base(Aspect.All(typeof(PositionComponent), typeof(MissileLauncherComponent)))
        {
            _missileTexture = textures[TextureID.Missile];
            _missileAnimation = new FixedFrameAnimation(10, 22).AddFrame(0, 0, 1f).AddFrame(1, 0, 1f);
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
            }
        }

        private void Missile_Collided(Entity missile, Entity entity)
        {
            if (entity.HasComponent<DroneComponent>())
                return;
            if (!entity.GetComponent<CollisionComponent>().Solid)
                return;
            if (!entity.HasComponent<HitComponent>())
                return;

            HitComponent hitComponent = entity.GetComponent<HitComponent>();
            if (hitComponent.HitSources.Contains(missile))
                return;
            hitComponent.HitSources.Add(missile);

            if (entity.HasComponent<CharacterComponent>())
            {
                CharacterComponent characterComponent = entity.GetComponent<CharacterComponent>();

                characterComponent.Power = 0;
            }
        }
    }
}
