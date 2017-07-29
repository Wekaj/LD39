using SFML.Graphics;
using SFML.System;

namespace LD39.Screens.Game
{
    internal sealed class GameScreen : IScreen
    {
        private readonly Context _context;

        public GameScreen(Context context)
        {
            _context = context;
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            return null;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
        }
    }
}
