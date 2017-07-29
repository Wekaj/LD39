using LD39.Systems;
using SFML.System;

namespace LD39.Extensions
{
    internal static class DirectionExtension
    {
        public static Vector2f ToVector(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return new Vector2f(1f, 0f);
                case Direction.Down:
                    return new Vector2f(0f, 1f);
                case Direction.Left:
                    return new Vector2f(-1f, 0f);
                case Direction.Up:
                    return new Vector2f(0f, -1f);
            }
            return new Vector2f();
        }
    }
}
