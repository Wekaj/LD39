using Artemis;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using SFML.Graphics;
using SFML.System;
using System.Linq;

namespace LD39.Systems
{
    internal sealed class DroneSystem : EntityUpdatingSystem
    {
        private const float _sightRadius = 64f, _pulseDistance = 20f, _speed = 40f, _acceleration = 200f;
        private readonly Texture _pulseTexture;
        private readonly FixedFrameAnimation _pulseAnimation;
        private Entity _character;

        public DroneSystem(TextureLoader textures) 
            : base(Aspect.All(typeof(PositionComponent), typeof(VelocityComponent), typeof(DroneComponent)))
        {
            _pulseTexture = textures[TextureID.Pulse];
            _pulseAnimation = new FixedFrameAnimation(48, 48);
            for (int i = 0; i < 12; i++)
                _pulseAnimation.AddFrame(i, 0, 1f);
        }

        protected override void Begin()
        {
            base.Begin();

            _character = EntityWorld.EntityManager.GetEntities(Aspect.All(typeof(CharacterComponent))).First();
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            DroneComponent droneComponent = entity.GetComponent<DroneComponent>();

            if (droneComponent.PulseTimer > DeltaTime)
                droneComponent.PulseTimer -= DeltaTime;
            else
                droneComponent.PulseTimer = Time.Zero;

            PositionComponent characterPositionComponent = _character.GetComponent<PositionComponent>();

            Vector2f difference = characterPositionComponent.Position - positionComponent.Position;

            float distance = difference.GetLength();
            if (distance < _sightRadius)
            {
                Vector2f targetVelocity = difference.Normalize() * _speed;
                velocityComponent.Velocity += (targetVelocity - velocityComponent.Velocity) * DeltaTime.AsSeconds();

                if (distance < _pulseDistance && droneComponent.PulseTimer <= Time.Zero)
                {
                    Entity pulse = EntityWorld.CreateEntity();
                    pulse.AddComponent(new PositionComponent(positionComponent.Position));
                    pulse.AddComponent(new SpriteComponent(new Sprite(_pulseTexture) { Position = new Vector2f(-24f, -24f) }, Layer.Below));

                    pulse.AddComponent(new AnimationComponent());
                    pulse.GetComponent<AnimationComponent>().Play(_pulseAnimation, Time.FromSeconds(0.4f), false);
                    pulse.GetComponent<AnimationComponent>().DestroyAtEnd = true;

                    //pulse.AddComponent(new LockComponent(entity, new Vector2f()));

                    pulse.AddComponent(new CollisionComponent(24f, false));
                    pulse.GetComponent<CollisionComponent>().Ignore = entity;
                    pulse.GetComponent<CollisionComponent>().Collided += (sender, e) => Pulse_Collided(pulse, e.Entity);

                    droneComponent.PulseTimer += Time.FromSeconds(3f);
                }
            }
        }

        private void Pulse_Collided(Entity pulse, Entity entity)
        {
            if (!entity.GetComponent<CollisionComponent>().Solid)
                return;

            if (entity.HasComponent<CharacterComponent>())
            {
                CharacterComponent characterComponent = entity.GetComponent<CharacterComponent>();

                if (characterComponent.Cooldown > Time.Zero)
                    return;

                characterComponent.Power -= 1f / 7f;
                characterComponent.Cooldown = Time.FromSeconds(0.5f);
            }

            PositionComponent positionComponent = pulse.GetComponent<PositionComponent>(),
                entityPositionComponent = entity.GetComponent<PositionComponent>();

            VelocityComponent entityVelocityComponent = entity.GetComponent<VelocityComponent>();

            Vector2f difference = entityPositionComponent.Position - positionComponent.Position;
            float distance = difference.GetLength();

            entityVelocityComponent.Velocity += (difference.Normalize() * (28f - distance)) * 5f;
        }
    }
}
