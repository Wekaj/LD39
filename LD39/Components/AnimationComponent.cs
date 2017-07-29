using Artemis.Interface;
using LD39.Animation;
using SFML.System;

namespace LD39.Components
{
    internal sealed class AnimationComponent : IComponent
    {
        public bool Playing { get; set; } = false;
        public IAnimation Animation { get; set; }
        public Time Duration { get; set; }
        public Time Timer { get; set; }
        public bool Looping { get; set; }

        public void Play(IAnimation animation, Time duration, bool looping = false)
        {
            if (Playing && animation == Animation && duration == Duration && Looping == looping)
                return;

            Playing = true;
            Animation = animation;
            Duration = duration;
            Timer = Time.Zero;
            Looping = looping;
        }

        public void Stop()
        {
            Playing = false;
        }
    }
}
