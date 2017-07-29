using SFML.Graphics;

namespace LD39.Animation
{
    internal interface IAnimation
    {
        void Animate(Sprite sprite, float progress);
    }
}
