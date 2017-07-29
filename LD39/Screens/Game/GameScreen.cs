using Artemis;
using Artemis.Manager;
using LD39.Systems;
using LD39.Tiles;
using SFML.Graphics;
using SFML.System;

namespace LD39.Screens.Game
{
    internal sealed class GameScreen : IScreen
    {
        private readonly Context _context;
        private readonly TileMap _background, _foreground;
        private readonly EntityWorld _entityWorld;

        public GameScreen(Context context)
        {
            _context = context;

            _entityWorld = new EntityWorld();
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _entityWorld.Update(deltaTime.AsMicroseconds() * 10);

            return null;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _entityWorld.Draw();
        }
    }
}
