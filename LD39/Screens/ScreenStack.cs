using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace LD39.Screens
{
    internal sealed class ScreenStack : Drawable
    {
        private readonly Stack<IScreen> _screens = new Stack<IScreen>();

        public ScreenStack(IScreen initialScreen)
        {
            _screens.Push(initialScreen);
        }

        public void Update(Time deltaTime)
        {
            ScreenChangeRequest request = _screens.Peek().Update(deltaTime);

            if (request != null)
            {
                switch (request.Type)
                {
                    case ScreenChangeRequestType.Pop:
                        _screens.Pop();
                        break;
                    case ScreenChangeRequestType.Push:
                        _screens.Push(request.Screen);
                        break;
                    case ScreenChangeRequestType.Replace:
                        _screens.Pop();
                        _screens.Push(request.Screen);
                        break;
                }
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _screens.Peek().Draw(target, states);
        }
    }
}
