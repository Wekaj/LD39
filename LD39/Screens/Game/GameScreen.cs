using Artemis;
using Artemis.Manager;
using LD39.Components;
using LD39.Resources;
using LD39.Systems;
using LD39.Tiles;
using SFML.Graphics;
using SFML.System;
using System;

namespace LD39.Screens.Game
{
    internal sealed class GameScreen : IScreen
    {
        private readonly Context _context;
        private readonly TileMap _background, _foreground;
        private readonly EntityWorld _entityWorld;
        private readonly Entity _character;
        private readonly Sprite _batteryBack, _batteryFill;
        private float _displayedPower = 1f;

        public GameScreen(Context context)
        {
            _context = context;

            int[,] backgroundMap = new int[,] 
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 2, 2, 2, 2, 0, 0, 0 },
                { 0, 2, 1, 1, 2, 0, 0, 0 },
                { 0, 2, 1, 1, 2, 0, 0, 0 },
                { 0, 2, 2, 1, 2, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            };

            _background = new TileMap(_context.Textures[TextureID.Tiles], 16, backgroundMap);
            _foreground = new TileMap(_context.Textures[TextureID.Tiles], 16, new int[backgroundMap.GetLength(0), backgroundMap.GetLength(1)]);

            _entityWorld = new EntityWorld();
            _entityWorld.SystemManager.SetSystem(new CharacterMovementSystem(_context.Actions, _context.Textures, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new LockSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new AnimationSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new DrawSystem(context.UpscaleTexture, _background, _foreground), GameLoopType.Draw);

            _character = _entityWorld.CreateEntity();
            _character.AddComponent(new CharacterComponent());
            _character.AddComponent(new PositionComponent());
            _character.AddComponent(new VelocityComponent());
            _character.AddComponent(new AnimationComponent());
            _character.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Character])
            {
                TextureRect = new IntRect(0, 0, 16, 32),
                Position = new Vector2f(-7.5f, -25f)
            }, Layer.Player));

            _batteryBack = new Sprite(_context.Textures[TextureID.BatteryBack]);
            _batteryBack.Position = new Vector2f(_context.UpscaleTexture.Size.X / 2f - _batteryBack.Texture.Size.X / 2f,
                _context.UpscaleTexture.Size.Y - 4f - _batteryBack.Texture.Size.Y);

            _batteryFill = new Sprite(_context.Textures[TextureID.BatteryFill]);
            _batteryFill.Position = _batteryBack.Position + new Vector2f(3f, 3f);
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _entityWorld.Update(deltaTime.AsMicroseconds() * 10);

            _displayedPower += (_character.GetComponent<CharacterComponent>().Power - _displayedPower) * 10f * deltaTime.AsSeconds();

            _batteryFill.TextureRect = new IntRect(0, 
                0, 
                (int)(_batteryFill.Texture.Size.X * _displayedPower), 
                (int)_batteryFill.Texture.Size.Y);

            return null;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _entityWorld.SystemManager.GetSystem<DrawSystem>()[0].RenderStates = states;

            _entityWorld.Draw();

            target.Draw(_batteryBack, states);
            target.Draw(_batteryFill, states);
        }
    }
}
