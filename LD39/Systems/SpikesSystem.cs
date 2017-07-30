using Artemis;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using SFML.Audio;
using SFML.System;
using System;
using System.Linq;

namespace LD39.Systems
{
    internal sealed class SpikesSystem : EntityUpdatingSystem
    {
        private readonly Time _spikeTime = Time.FromSeconds(2f);
        private readonly float _soundDistance = 160f;
        private readonly int _tileSize;
        private readonly FixedFrameAnimation _extendAnimation, _retractAnimation;
        private readonly Sound _spikesUp, _spikesDown;
        private Entity _character;

        public SpikesSystem(int tileSize, SoundBufferLoader soundBuffers) 
            : base(Aspect.All(typeof(PositionComponent), typeof(SpriteComponent), typeof(SpikesComponent), typeof(AnimationComponent)))
        {
            _tileSize = tileSize;
            _extendAnimation = new FixedFrameAnimation(16, 16).AddFrame(1, 0, 1f).AddFrame(2, 0, 1f).AddFrame(3, 0, 1f).AddFrame(4, 0, 1f);
            _retractAnimation = new FixedFrameAnimation(16, 16).AddFrame(3, 0, 1f).AddFrame(2, 0, 1f).AddFrame(1, 0, 1f).AddFrame(0, 0, 1f);
            _spikesUp = new Sound(soundBuffers[SoundBufferID.SpikesUp]) { Volume = 21f };
            _spikesDown = new Sound(soundBuffers[SoundBufferID.SpikesDown]) { Volume = 21f };
        }

        protected override void Begin()
        {
            base.Begin();

            _character = EntityWorld.EntityManager.GetEntities(Aspect.All(typeof(CharacterComponent))).First();
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

                float distance = (positionComponent.Position - characterPositionComponent.Position).GetLength();

                if (spikesComponent.Extended)
                {
                    animationComponent.Play(_extendAnimation, Time.FromSeconds(0.1f));
                    _spikesUp.Volume = 21f * Math.Max(_soundDistance - distance, 0f) / _soundDistance;
                    if (_spikesUp.Volume > 0f)
                        _spikesUp.Play();
                }
                else
                {
                    animationComponent.Play(_retractAnimation, Time.FromSeconds(0.1f));
                    _spikesDown.Volume = 21f * Math.Max(_soundDistance - distance, 0f) / _soundDistance;
                    if (_spikesDown.Volume > 0f)
                        _spikesDown.Play();
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
