using SFML.Graphics;
using SFML.System;

namespace LD39.Screens
{
    internal interface IScreen : Drawable
    {
        ScreenChangeRequest Update(Time deltaTime);
    }
}
