using Artemis;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using SFML.Audio;
using SFML.System;
using System.Linq;

namespace LD39.Systems
{
    internal sealed class SpikesSystem : EntityUpdatingSystem
    {
        private readonly Time _spikeTime = Time.FromSeconds(2f);
        private readonly float _soundDistance = 120f;
        private readonly int _tileSize;
        private readonly FixedFrameAnimation _extendAnimation, _retractAnimation;
        private readonly Sound _spikesUpSound, _spikesDownSound;
        private Entity _character;
        private bool _up = true, _down = true;

        public SpikesSystem(int tileSize, SoundBufferLoader soundBuffers) 
            : base(Aspect.All(typeof(PositionComponent), typeof(SpriteComponent), typeof(SpikesComponent), typeof(AnimationComponent)))
        {
            _tileSize = tileSize;
            _extendAnimation = new FixedFrameAnimation(16, 16).AddFrame(1, 0, 1f).AddFrame(2, 0, 1f).AddFrame(3, 0, 1f).AddFrame(4, 0, 1f);
            _retractAnimation = new FixedFrameAnimation(16, 16).AddFrame(3, 0, 1f).AddFrame(2, 0, 1f).AddFrame(1, 0, 1f).AddFrame(0, 0, 1f);
            _spikesUpSound = new Sound(soundBuffers[SoundBufferID.SpikesUp]) { Volume = 21f };
            _spikesDownSound = new Sound(soundBuffers[SoundBufferID.SpikesDown]) { Volume = 21f };
        }

        protected override void Begin()
        {
            base.Begin();

            _character = EntityWorld.EntityManager.GetEntities(Aspect.All(typeof(CharacterComponent))).First();
            _up = true;
            _down = true;
        }

        public override void Process(Entity entity)
        {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();
            SpikesComponent spikesComponent = entity.GetComponent<SpikesComponent>();
            AnimationComponent animationComponent = entity.GetComponent<AnimationComponent>();

            positionComponent.Position = (Vector2f)spikesComponent.Tile * _tileSize;

            PositionComponent characterPositionComponent = _character.GetComponent<PositionComponent>();

            if (spikesComponent.Timer > DeltaTime)
                spikesComponent.Timer -= DeltaTime;
            else
            {
                spikesComponent.Timer += _spikeTime - DeltaTime;
                spikesComponent.Extended = !spikesComponent.Extended;

                float distance = (characterPositionComponent.Position - positionComponent.Position).GetLength();
                if (spikesComponent.Extended)
                {
                    animationComponent.Play(_extendAnimation, Time.FromSeconds(0.1f));
                    if (distance < _soundDistance && _up)
                    {
                        _spikesUpSound.Play();
                        _up = false;
                    }
                }
                else
                {
                    animationComponent.Play(_retractAnimation, Time.FromSeconds(0.1f));
                    if (distance < _soundDistance && _down)
                    {
                        _spikesDownSound.Play();
                        _down = false;
                    }
                }
            }

            if (spikesComponent.Extended)
            {
                Vector2i tile = (Vector2i)(characterPositionComponent.Position / _tileSize).Floor();
                if (tile == spikesComponent.Tile)
                    _character.GetComponent<CharacterComponent>().Power = 0f;
            }
        }
    }
}
