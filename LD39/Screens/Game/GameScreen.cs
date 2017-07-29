using Artemis;
using Artemis.Manager;
using LD39.Animation;
using LD39.Components;
using LD39.Input;
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
        private readonly FixedFrameAnimation _slashAnimation;

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
            _entityWorld.SystemManager.SetSystem(new CharacterMovementSystem(_context.Actions), GameLoopType.Update);
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

            _slashAnimation = new FixedFrameAnimation(32, 32);
            for (int i = 0; i < 15; i++)
                _slashAnimation.AddFrame(i, 0, 1f);

            _context.Actions[ActionID.Attack].Pressed += Attack_Pressed;
        }

        private void Attack_Pressed(object sender, EventArgs e)
        {
            PositionComponent positionComponent = _character.GetComponent<PositionComponent>();

            Entity slash = _entityWorld.CreateEntity();
            slash.AddComponent(new PositionComponent(positionComponent.Position));
            slash.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Slash]) { Position = new Vector2f(-16f, -8f) }, Layer.Effects));
            slash.AddComponent(new AnimationComponent());
            slash.AddComponent(new LockComponent(_character));
            slash.GetComponent<AnimationComponent>().Play(_slashAnimation, Time.FromSeconds(0.5f), false);
            slash.GetComponent<AnimationComponent>().DestroyAtEnd = true;
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _entityWorld.Update(deltaTime.AsMicroseconds() * 10);

            return null;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _entityWorld.SystemManager.GetSystem<DrawSystem>()[0].RenderStates = states;

            _entityWorld.Draw();
        }
    }
}
