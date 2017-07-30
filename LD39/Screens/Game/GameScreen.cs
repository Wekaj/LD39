﻿using Artemis;
using Artemis.Manager;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using LD39.Screens.Recharge;
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
        private readonly FixedFrameAnimation _droneAnimation, _stationAnimation;
        private readonly int _station;
        private float _displayedPower = 1f;
        private int _rechargeStation = -1;

        public GameScreen(Context context, int stationID)
        {
            _context = context;
            _station = stationID;

            int[,] backgroundMap = new int[,] 
            {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 2, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 2, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 2, 3, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 2, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 2, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 2, 1, 1, 1, 4, 1, 1, 0, 0 },
                { 0, 2, 1, 0, 0, 2, 1, 1, 0, 0 },
                { 0, 2, 3, 0, 0, 2, 1, 1, 0, 0 },
                { 2, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 2, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 2, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 2, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
            };

            bool[,] collisions = new bool[backgroundMap.GetLength(0), backgroundMap.GetLength(1)];
            for (int y = 0; y < collisions.GetLength(1); y++)
                for (int x = 0; x < collisions.GetLength(0); x++)
                    collisions[x, y] = backgroundMap[x, y] == 2;

            _background = new TileMap(_context.Textures[TextureID.Tiles], 16, backgroundMap);
            _foreground = new TileMap(_context.Textures[TextureID.Tiles], 16, new int[backgroundMap.GetLength(0), backgroundMap.GetLength(1)]);

            _entityWorld = new EntityWorld();
            _entityWorld.SystemManager.SetSystem(new CharacterMovementSystem(_context.Actions, _context.Textures, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new DroneSystem(_context.Textures), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new CollisionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new TileCollisionSystem(collisions, 16f, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new FrictionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new LockSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new AnimationSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new HealthSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new StationSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.GetSystem<StationSystem>()[0].StationTouched += StationSystem_StationTouched;
            _entityWorld.SystemManager.SetSystem(new DrawSystem(context.UpscaleTexture, _background, _foreground), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new HealthDrawSystem(context.UpscaleTexture, context.Textures), GameLoopType.Draw);

            _stationAnimation = new FixedFrameAnimation(16, 16);
            _stationAnimation.AddFrame(0, 0, 1f);
            for (int i = 1; i < 6; i++)
                _stationAnimation.AddFrame(i, 0, 0.05f);
            _stationAnimation.AddFrame(6, 0, 1f);
            for (int i = 7; i < 12; i++)
                _stationAnimation.AddFrame(i, 0, 0.05f);

            Entity station = _entityWorld.CreateEntity();
            station.AddComponent(new PositionComponent(48f, 48f));
            station.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Station]) { Position = new Vector2f(-8f, -8f) }, Layer.Floor));
            station.AddComponent(new AnimationComponent());
            station.AddComponent(new StationComponent(0));
            station.GetComponent<AnimationComponent>().Play(_stationAnimation, Time.FromSeconds(3f), true);

            _character = _entityWorld.CreateEntity();
            _character.AddComponent(new CharacterComponent());
            foreach (Entity stationEntity in _entityWorld.EntityManager.GetEntities(Aspect.All(typeof(PositionComponent), typeof(StationComponent))))
            {
                PositionComponent positionComponent = stationEntity.GetComponent<PositionComponent>();
                StationComponent stationComponent = stationEntity.GetComponent<StationComponent>();

                if (stationComponent.ID == stationID)
                {
                    _character.AddComponent(new PositionComponent(positionComponent.Position));
                    break;
                }
            }
            _character.AddComponent(new VelocityComponent());
            _character.AddComponent(new AnimationComponent());
            _character.AddComponent(new TileCollisionComponent(2f));
            _character.AddComponent(new CollisionComponent(4f));
            _character.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Character])
            {
                TextureRect = new IntRect(0, 0, 16, 32),
                Position = new Vector2f(-8f, -25f)
            }, Layer.Player));

            _droneAnimation = new FixedFrameAnimation(16, 16).AddFrame(0, 0, 1f).AddFrame(1, 0, 0.1f).AddFrame(2, 0, 1f).AddFrame(3, 0, 0.1f);

            Random random = new Random();
            for (int i = 0; i < 5; i++)
            {
                Entity drone = _entityWorld.CreateEntity();
                drone.AddComponent(new PositionComponent(160f + random.Next(64), 16f + random.Next(48)));
                drone.AddComponent(new VelocityComponent());
                drone.AddComponent(new FrictionComponent(10f));
                drone.AddComponent(new AnimationComponent());
                drone.GetComponent<AnimationComponent>().Play(_droneAnimation, Time.FromSeconds(2f), true);
                drone.AddComponent(new TileCollisionComponent(2f));
                drone.AddComponent(new CollisionComponent(2f));
                drone.AddComponent(new DroneComponent());
                drone.AddComponent(new HealthComponent(10, 28f));
                drone.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Drone])
                {
                    TextureRect = new IntRect(0, 0, 16, 16),
                    Position = new Vector2f(-8f, -16f)
                }, Layer.Player));
            }

            Entity character2 = _entityWorld.CreateEntity();
            character2.AddComponent(new PositionComponent(128f, 128f));
            character2.AddComponent(new VelocityComponent());
            character2.AddComponent(new AnimationComponent());
            character2.AddComponent(new TileCollisionComponent(2f));
            character2.AddComponent(new CollisionComponent(4f));
            character2.AddComponent(new FrictionComponent(500f));
            character2.AddComponent(new HealthComponent(10, 28f));
            character2.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Character])
            {
                TextureRect = new IntRect(0, 0, 16, 32),
                Position = new Vector2f(-8f, -25f)
            }, Layer.Player));

            _batteryBack = new Sprite(_context.Textures[TextureID.BatteryBack]);
            _batteryBack.Position = new Vector2f(_context.UpscaleTexture.Size.X / 2f - _batteryBack.Texture.Size.X / 2f,
                _context.UpscaleTexture.Size.Y - 4f - _batteryBack.Texture.Size.Y);

            _batteryFill = new Sprite(_context.Textures[TextureID.BatteryFill]);
            _batteryFill.Position = _batteryBack.Position + new Vector2f(3f, 3f);

            _entityWorld.Update(0);
        }

        private void StationSystem_StationTouched(object sender, StationEventArgs e)
        {
            _rechargeStation = e.ID;
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _entityWorld.Update(deltaTime.AsMicroseconds() * 10);

            if (_rechargeStation >= 0)
                return ScreenChangeRequest.Replace(new RechargeScreen(_context, _rechargeStation));

            CharacterComponent characterComponent = _character.GetComponent<CharacterComponent>();

            if (characterComponent.Power <= 0f)
                return ScreenChangeRequest.Replace(new RechargeScreen(_context, _station));

            characterComponent.Power = Math.Min(1f, characterComponent.Power);

            float power = Math.Max(characterComponent.Power, 0f);
            _displayedPower += (power - _displayedPower) * 10f * deltaTime.AsSeconds();

            _batteryFill.TextureRect = new IntRect(0, 
                0, 
                (int)(_batteryFill.Texture.Size.X * _displayedPower), 
                (int)_batteryFill.Texture.Size.Y);

            return null;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            View view = _context.UpscaleTexture.GetView();
            view.Center = _character.GetComponent<PositionComponent>().Position.Floor();
            _context.UpscaleTexture.SetView(view);

            _entityWorld.SystemManager.GetSystem<DrawSystem>()[0].RenderStates = states;
            _entityWorld.SystemManager.GetSystem<HealthDrawSystem>()[0].RenderStates = states;

            _entityWorld.Draw();

            _context.UpscaleTexture.SetView(_context.UpscaleTexture.DefaultView);

            target.Draw(_batteryBack, states);
            target.Draw(_batteryFill, states);
        }
    }
}
