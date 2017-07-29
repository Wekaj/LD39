using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace LD39.Animation
{
    internal sealed class FixedFrameAnimation : IAnimation
    {
        private readonly int _width, _height;
        private readonly List<Frame> _frames = new List<Frame>();
        private float _duration;

        public FixedFrameAnimation(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public FixedFrameAnimation AddFrame(int x, int y, float duration)
        {
            _duration += duration;
            _frames.Add(new Frame(new Vector2i(x, y), duration));
            return this;
        }

        public void Animate(Sprite sprite, float progress)
        {
            progress *= _duration;

            for (int i = 0; i < _frames.Count; i++)
            {
                Frame frame = _frames[i];

                progress -= frame.Duration;

                if (progress <= 0f)
                {
                    sprite.TextureRect = new IntRect(frame.Position.X * _width, frame.Position.Y * _height, 
                        _width, _height);
                    break;
                }
            }
        }

        private struct Frame
        {
            public Frame(Vector2i position, float duration)
            {
                Position = position;
                Duration = duration;
            }

            public Vector2i Position { get; }
            public float Duration { get; }
        }
    }
}
