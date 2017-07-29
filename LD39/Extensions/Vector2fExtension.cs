using SFML.System;
using System;

namespace LD39.Extensions
{
    internal static class Vector2fExtension
    {
        public static float GetLength(this Vector2f vector)
        {
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        public static Vector2f Normalize(this Vector2f vector)
        {
            if (vector.X == 0f && vector.Y == 0f)
                return vector;
            return vector / vector.GetLength();
        }

        public static Vector2f Floor(this Vector2f vector)
        {
            return new Vector2f((float)Math.Floor(vector.X), (float)Math.Floor(vector.Y));
        }
    }
}
