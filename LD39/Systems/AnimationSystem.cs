using Artemis;
using LD39.Components;

namespace LD39.Systems
{
    internal sealed class AnimationSystem : EntityUpdatingSystem
    {
        public AnimationSystem() 
            : base(Aspect.All(typeof(AnimationComponent), typeof(SpriteComponent)))
        {
        }

        public override void Process(Entity entity)
        {
            AnimationComponent animationComponent = entity.GetComponent<AnimationComponent>();

            if (!animationComponent.Playing)
                return;

            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();

            animationComponent.Timer += DeltaTime;
            if (animationComponent.Looping)
            {
                while (animationComponent.Timer > animationComponent.Duration)
                    animationComponent.Timer -= animationComponent.Duration;
            }
            else if (animationComponent.Timer >= animationComponent.Duration)
            {
                animationComponent.Animation.Animate(spriteComponent.Sprite, 1f);
                animationComponent.Playing = false;
                return;
            }

            animationComponent.Animation.Animate(spriteComponent.Sprite, 
                animationComponent.Timer.AsSeconds() / animationComponent.Duration.AsSeconds());
        }
    }
}
